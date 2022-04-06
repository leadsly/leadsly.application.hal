﻿using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Campaigns.SendConnections;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class SendConnectionsProvider : ISendConnectionsProvider
    {
        public SendConnectionsProvider(
            ILogger<SendConnectionsProvider> logger,
            IWebDriverProvider webDriverProvider,
            ILinkedInHomePage linkedInHomePage,
            ILinkedInSearchPage linkedInSearchPage
            )
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _linkedInSearchPage = linkedInSearchPage;
            _linkedInHomePage = linkedInHomePage;
        }
        
        private readonly ILogger<SendConnectionsProvider> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;

        public HalOperationResult<T> ExecutePhase<T>(SendConnectionsBody message, IList<SentConnectionsUrlStatusRequest> sentConnectionsUrlStatusPayload) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.Connect,
                ChromeProfileName = message.ChromeProfileName
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            return SendConnections<T>(webDriver, message, sentConnectionsUrlStatusPayload);
        }

        private HalOperationResult<T> SendConnections<T>(IWebDriver webDriver, SendConnectionsBody message, IList<SentConnectionsUrlStatusRequest> sentConnectionsUrlStatusPayload)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            SentConnectionsUrlStatusRequest sentConnectionsUrlStatus = sentConnectionsUrlStatusPayload.FirstOrDefault();
            if (sentConnectionsUrlStatus == null)
            {
                _logger.LogWarning("No sent connections url status provided! Web driver does not know where to start sending connections!");
                return result;
            }

            // only open up a new tab if we aren't trying to re-use a window handle from before
            if (sentConnectionsUrlStatus.WindowHandleId != null)
            {
                result = _webDriverProvider.SwitchTo<T>(webDriver, sentConnectionsUrlStatus.WindowHandleId);
                if(result.Succeeded == false)
                {
                    result = _webDriverProvider.NewTab<T>(webDriver);
                    if (result.Succeeded == false)
                    {
                        return result;
                    }
                    // if the switch has failed, its possible we have a stale window reference so just go to a new page
                    result = GoToPage<T>(webDriver, sentConnectionsUrlStatus.CurrentUrl);
                    if (result.Succeeded == false)
                    {
                        return result;
                    }
                }
            }
            else
            {
                result = _webDriverProvider.NewTab<T>(webDriver);
                if (result.Succeeded == false)
                {
                    return result;
                }

                result = GoToPage<T>(webDriver, sentConnectionsUrlStatus.CurrentUrl);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }

            result = SendConnectionRequests<T>(webDriver, message.SendConnectionsStage.ConnectionsLimit, message.CampaignId, sentConnectionsUrlStatusPayload);
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> SendConnectionRequests<T>(IWebDriver webDriver, int stageConnectionsLimit, string campaignId, IList<SentConnectionsUrlStatusRequest> sentConnectionsUrlStatusPayload)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // we skip one because by the time we are in this method, we have already navigated to the first url
            Queue<SentConnectionsUrlStatusRequest> allSentConnectionsUrlStatuses = new(sentConnectionsUrlStatusPayload);
            IList<SentConnectionsUrlStatusRequest> updatedSentConnectionsUrlStatusRequests = new List<SentConnectionsUrlStatusRequest>();
            SentConnectionsUrlStatusRequest currentSentConnectionsUrlStatus = allSentConnectionsUrlStatuses.Dequeue();

            List<CampaignProspectRequest> connectedProspects = new();
            while (stageConnectionsLimit != 0)
            {
                bool isNoSearchResultsContainerDisplayed = _linkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
                if (isNoSearchResultsContainerDisplayed == true)
                {
                    _logger.LogWarning("[SendConnectionRequests]: No search results container displayed. Attempting to find and click retry serach button");
                    HalOperationResult<IOperationResponse> retrySearchResult = _linkedInSearchPage.ClickRetrySearch<IOperationResponse>(webDriver);
                    if (retrySearchResult.Succeeded == false)
                    {
                        _logger.LogError("[SendConnectionRequests]: Failed to recover from no search results container being displayed");
                        return result;
                    }
                }

                HalOperationResult<IGatherProspects> gatherProspectsResult = _linkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
                if (gatherProspectsResult.Succeeded == false)
                {
                    _logger.LogError("Failed to gather prospects from the search results hitlist");
                    return result;
                }

                //IList<IWebElement> campaignProspectsAsWebElements = new List<IWebElement>();
                // find prospects that have an action button // .entity-result__actions
                foreach (IWebElement prospect in gatherProspectsResult.Value.ProspectElements)
                {
                    if(stageConnectionsLimit == 0)
                    {
                        // update sent connections url status
                        currentSentConnectionsUrlStatus.CurrentUrl = webDriver.Url;
                        currentSentConnectionsUrlStatus.StartedCrawling = true;
                        currentSentConnectionsUrlStatus.LastActivityTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        currentSentConnectionsUrlStatus.WindowHandleId = webDriver.CurrentWindowHandle;
                        updatedSentConnectionsUrlStatusRequests.Add(currentSentConnectionsUrlStatus);

                        goto finalize;
                    }

                    _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");
                    result = _linkedInSearchPage.SendConnectionRequest<T>(prospect);
                    if (result.Succeeded == false)
                    {
                        continue;
                    }

                    result = _linkedInSearchPage.ClickSendInModal<T>(webDriver);
                    if(result.Succeeded == false)
                    {
                        continue;
                    }
                    //campaignProspectsAsWebElements.Add(prospect);
                    connectedProspects.Add(CreateCampaignProspects(prospect, campaignId));

                    stageConnectionsLimit -= 1;
                }

                //connectedProspects.AddRange(CreateCampaignProspects(campaignProspectsAsWebElements, campaignId));                

                bool nextDisabled = _linkedInSearchPage.IsNextButtonDisabled(webDriver);
                if (nextDisabled)
                {
                    currentSentConnectionsUrlStatus.CurrentUrl = webDriver.Url;
                    currentSentConnectionsUrlStatus.StartedCrawling = true;
                    currentSentConnectionsUrlStatus.FinishedCrawling = true;
                    currentSentConnectionsUrlStatus.WindowHandleId = string.Empty;
                    currentSentConnectionsUrlStatus.LastActivityTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    updatedSentConnectionsUrlStatusRequests.Add(currentSentConnectionsUrlStatus);

                    if(allSentConnectionsUrlStatuses.Count > 0)
                    {
                        currentSentConnectionsUrlStatus = allSentConnectionsUrlStatuses.Dequeue();

                        result = GoToPage<T>(webDriver, currentSentConnectionsUrlStatus.CurrentUrl);
                        if (result.Succeeded == false)
                        {
                            return result;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // go to the next page
                HalOperationResult<IOperationResponse> clickNextResult = _linkedInSearchPage.ClickNext<IOperationResponse>(webDriver);
                if (clickNextResult.Succeeded == false)
                {
                    _logger.LogError("[SendConnectionRequests]: Failed to navigate to the next page");
                    return result;
                }
            }

            ISendConnectionsPayload campaignProspectsPayload = default;
            finalize:
            {
                campaignProspectsPayload = new SendConnectionsPayload
                {
                    CampaignProspects = connectedProspects,
                    SentConnectionsUrlStatuses = updatedSentConnectionsUrlStatusRequests
                };
            }

            result.Value = (T)campaignProspectsPayload;
            result.Succeeded = true;
            return result;
        }

        private IList<CampaignProspectRequest> CreateCampaignProspects(IList<IWebElement> prospects, string campaignId)
        {
            IList<CampaignProspectRequest> campaignProspects = new List<CampaignProspectRequest>();

            foreach (IWebElement webElement in prospects)
            {
                campaignProspects.Add(new()
                {
                    ConnectionSentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = GetProspectsName(webElement),
                    ProfileUrl = GetProspectsProfileUrl(webElement),
                    CampaignId = campaignId
                });
            }

            return campaignProspects;
        }

        private CampaignProspectRequest CreateCampaignProspects(IWebElement prospect, string campaignId)
        {
            return new CampaignProspectRequest()
            {
                ConnectionSentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Name = GetProspectsName(prospect),
                ProfileUrl = GetProspectsProfileUrl(prospect),
                CampaignId = campaignId
            };
        }

        private string GetProspectsName(IWebElement webElement)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement prospectNameSpan = webElement.FindElement(By.CssSelector(".entity-result__title-text"));
                try
                {
                    IWebElement thirdConnectionProspectName = prospectNameSpan.FindElement(By.CssSelector("span[aria-hidden=true]"));
                    prospectName = thirdConnectionProspectName.Text;
                }
                catch (Exception ex)
                {
                    // ignore the error and proceed
                    prospectName = prospectNameSpan.Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects name");
            }

            return prospectName;
        }

        private string GetProspectsProfileUrl(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            string userName = innerText[0] ?? string.Empty;
            if (userName == "LinkedIn Member")
            {
                // this means we don't have access to user's profile
                return string.Empty;
            }

            string profileUrl = string.Empty;
            try
            {
                IWebElement anchorTag = webElement.FindElement(By.CssSelector(".app-aware-link"));
                profileUrl = anchorTag.GetAttribute("href");
                profileUrl = profileUrl.Split('?').FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects profile url");
            }

            return profileUrl;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                // first navigate to messages
                result = _linkedInHomePage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }
    }
}
