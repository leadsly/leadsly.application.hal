using Domain.Facades.Interfaces;
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
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Providers.Campaigns
{
    public class SendConnectionsProvider : ISendConnectionsProvider
    {
        public SendConnectionsProvider(
            ILogger<SendConnectionsProvider> logger,
            IWebDriverProvider webDriverProvider,
            ICampaignProspectsService campaignProspectService,
            IHumanBehaviorService humanBehaviorService,
            ILinkedInPageFacade linkedInPageFacade
            )
        {
            _campaignProspectsService = campaignProspectService;
            _humanBehaviorService = humanBehaviorService;
            _webDriverProvider = webDriverProvider;
            _linkedInPageFacade = linkedInPageFacade;
            _logger = logger;            
        }

        private readonly ICampaignProspectsService _campaignProspectsService;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<SendConnectionsProvider> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInPageFacade _linkedInPageFacade;

        public HalOperationResult<T> ExecutePhase<T>(SendConnectionsBody message, IList<SearchUrlDetailsRequest> sentConnectionsUrlStatusPayload) where T : IOperationResponse
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

            result = SendConnections<T>(webDriver, message, sentConnectionsUrlStatusPayload);

            if(result.Succeeded == false)
            {
                return result;
            }

            _webDriverProvider.CloseBrowser<T>(BrowserPurpose.Connect);

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> SendConnections<T>(IWebDriver webDriver, SendConnectionsBody message, IList<SearchUrlDetailsRequest> sentConnectionsUrlStatusPayload)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            SearchUrlDetailsRequest sentConnectionsUrlStatus = sentConnectionsUrlStatusPayload.FirstOrDefault();
            if (sentConnectionsUrlStatus == null)
            {
                _logger.LogWarning("No sent connections url status provided! Web driver does not know where to start sending connections!");
                return result;
            }

            // only open up a new tab if we aren't trying to re-use a window handle from before
            if (sentConnectionsUrlStatus.WindowHandleId != string.Empty)
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

            result = SendConnectionRequests<T>(webDriver, message.SendConnectionsStage.ConnectionsLimit, message.CampaignId, message.TimeZoneId, sentConnectionsUrlStatusPayload);
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> SendConnectionRequests<T>(IWebDriver webDriver, int stageConnectionsLimit, string campaignId, string timeZoneId, IList<SearchUrlDetailsRequest> sentConnectionsUrlStatusPayload)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // we skip one because by the time we are in this method, we have already navigated to the first url
            Queue<SearchUrlDetailsRequest> allSentConnectionsUrlStatuses = new(sentConnectionsUrlStatusPayload);
            IList<SearchUrlDetailsRequest> updatedSentConnectionsUrlStatusRequests = new List<SearchUrlDetailsRequest>();
            SearchUrlDetailsRequest currentSentConnectionsUrlStatus = allSentConnectionsUrlStatuses.Dequeue();

            List<CampaignProspectRequest> connectedProspects = new();
            while (stageConnectionsLimit != 0)
            {
                HalOperationResult<IOperationResponse> waitForSearchResultsToFinishLoading = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<IOperationResponse>(webDriver);
                if (waitForSearchResultsToFinishLoading.Succeeded == false)
                {
                    return result;
                }

                bool isNoSearchResultsContainerDisplayed = _linkedInPageFacade.LinkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
                if (isNoSearchResultsContainerDisplayed == true)
                {
                    _logger.LogWarning("[SendConnectionRequests]: No search results container displayed. Attempting to find and click retry serach button");
                    HalOperationResult<IOperationResponse> retrySearchResult = _linkedInPageFacade.LinkedInSearchPage.ClickRetrySearch<IOperationResponse>(webDriver);
                    if (retrySearchResult.Succeeded == false)
                    {
                        _logger.LogError("[SendConnectionRequests]: Failed to recover from no search results container being displayed");
                        return result;
                    }
                }

                HalOperationResult<IGatherProspects> gatherProspectsResult = _linkedInPageFacade.LinkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
                if (gatherProspectsResult.Succeeded == false)
                {
                    _logger.LogError("Failed to gather prospects from the search results hitlist");
                    return result;
                }

                foreach (IWebElement prospect in gatherProspectsResult.Value.ProspectElements)
                {
                    if(stageConnectionsLimit == 0)
                    {
                        // update sent connections url status
                        currentSentConnectionsUrlStatus.CurrentUrl = webDriver.Url;
                        currentSentConnectionsUrlStatus.StartedCrawling = true;
                        currentSentConnectionsUrlStatus.LastActivityTimestamp = new DateTimeOffset(new DateTimeWithZone(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)).LocalTime).ToUnixTimeSeconds();
                        currentSentConnectionsUrlStatus.WindowHandleId = webDriver.CurrentWindowHandle;
                        updatedSentConnectionsUrlStatusRequests.Add(currentSentConnectionsUrlStatus);

                        goto finalize;
                    }

                    _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");

                    IWebElement searchResultsHeader = _linkedInPageFacade.LinkedInSearchPage.ResultsHeader(webDriver);
                    _humanBehaviorService.RandomClickElement(searchResultsHeader);

                    _humanBehaviorService.RandomWaitMilliSeconds(700, 3000);
                    result = _linkedInPageFacade.LinkedInSearchPage.SendConnectionRequest<T>(prospect);
                    if (result.Succeeded == false)
                    {
                        continue;
                    }

                    IWebElement modalContent = _linkedInPageFacade.LinkedInSearchPage.GetCustomizeThisInvitationModalContent(webDriver);
                    _humanBehaviorService.RandomClickElement(modalContent);

                    _humanBehaviorService.RandomWaitMilliSeconds(700, 1400);
                    result = _linkedInPageFacade.LinkedInSearchPage.ClickSendInModal<T>(webDriver);
                    if(result.Succeeded == false)
                    {
                        continue;
                    }
                    CampaignProspectRequest campaignProspectRequest = _campaignProspectsService.CreateCampaignProspects(prospect, campaignId);
                    connectedProspects.Add(campaignProspectRequest);

                    stageConnectionsLimit -= 1;
                }

                result = _linkedInPageFacade.LinkedInSearchPage.ScrollFooterIntoView<T>(webDriver);
                if(result.Succeeded == false)
                {
                    return result;
                }

                bool nextDisabled = _linkedInPageFacade.LinkedInSearchPage.IsNextButtonDisabled(webDriver);
                if (nextDisabled)
                {
                    currentSentConnectionsUrlStatus.CurrentUrl = webDriver.Url;
                    currentSentConnectionsUrlStatus.StartedCrawling = true;
                    currentSentConnectionsUrlStatus.FinishedCrawling = true;
                    currentSentConnectionsUrlStatus.WindowHandleId = string.Empty;
                    currentSentConnectionsUrlStatus.LastActivityTimestamp = new DateTimeOffset(new DateTimeWithZone(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)).LocalTime).ToUnixTimeSeconds();
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
                HalOperationResult<IOperationResponse> clickNextResult = _linkedInPageFacade.LinkedInSearchPage.ClickNext<IOperationResponse>(webDriver);
                if (clickNextResult.Succeeded == false)
                {
                    _logger.LogError("[SendConnectionRequests]: Failed to navigate to the next page");
                    return result;
                }

                HalOperationResult<IOperationResponse> waitForResultsOperation = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<IOperationResponse>(webDriver);
                if (waitForResultsOperation.Succeeded == false)
                {
                    _logger.LogError("Search results never finished loading.");
                    break;
                }

            }

            ISendConnectionsPayload campaignProspectsPayload = default;
            finalize:
            {
                campaignProspectsPayload = new SendConnectionsPayload
                {
                    CampaignProspects = connectedProspects,
                    SearchUrlDetailsRequests = updatedSentConnectionsUrlStatusRequests
                };
            }

            result.Value = (T)campaignProspectsPayload;
            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                // first navigate to messages
                result = _linkedInPageFacade.LinkedInHomePage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }
    }
}
