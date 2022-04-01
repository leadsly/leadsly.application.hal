﻿using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.interfaces;
using Leadsly.Application.Model.Campaigns.ProspectList;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
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
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ProspectListProvider : IProspectListProvider
    {
        public ProspectListProvider(
            ILinkedInHomePage linkedInHomePage,            
            ILogger<ProspectListProvider> logger,
            IWebDriverProvider webDriverProvider,                                                
            ILinkedInSearchPage linkedInSearchPage)
        {
            _logger = logger;
            _linkedInSearchPage = linkedInSearchPage;
            _webDriverProvider = webDriverProvider;            
            _linkedInHomePage = linkedInHomePage;                
        }

        private readonly IWebDriverProvider _webDriverProvider;        
        private readonly ILogger<ProspectListProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;        
        private readonly ILinkedInSearchPage _linkedInSearchPage;        

        #region Execute ProspectList Phase

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ProspectListBody message) 
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.ProspectList,
                ChromeProfileName = message.ChromeProfile,
                PageUrls = message.SearchUrls
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            return ProspectList<T>(webDriver, message);
        }
        private HalOperationResult<T> ProspectList<T>(IWebDriver webDriver, ProspectListBody message)
            where T : IOperationResponse
        {
            string defaultWindowHandle = webDriver.CurrentWindowHandle;

            HalOperationResult<T> result = new();
            IList<PrimaryProspectRequest> prospects = new List<PrimaryProspectRequest>();            
            foreach (string searchUrl in message.SearchUrls)
            {
                result = _webDriverProvider.NewTab<T>(webDriver);
                if(result.Succeeded == false)
                {
                    return result;
                }

                result = GoToPage<T>(webDriver, searchUrl);
                if(result.Succeeded == false)
                {
                    return result;
                }

                result = _linkedInSearchPage.GetTotalSearchResults<T>(webDriver);
                if (result.Succeeded == false)
                {
                    result.Failures.Add(new()
                    {
                        Code = Codes.WEBDRIVER_ERROR,
                        Reason = "Failed to extract number of search results on the search url"
                    });
                    return result;
                }
                int totalResults = ((IGetTotalNumberOfResults)result.Value).NumberOfResults;

                _webDriverProvider.CloseTab<T>(BrowserPurpose.ProspectList, webDriver.CurrentWindowHandle);
                _webDriverProvider.SwitchTo<T>(webDriver, defaultWindowHandle);

                IList<PrimaryProspectRequest> primaryProspects = CollectProspects(webDriver, searchUrl, totalResults, message.PrimaryProspectListId);

                if (primaryProspects != null)
                {
                    prospects = prospects.Concat(primaryProspects).ToList();
                }
            }

            IPrimaryProspectListPayload primaryProspectsPayload = new PrimaryProspectListPayload
            {
                Prospects = prospects
            };

            result = _webDriverProvider.SwitchTo<T>(webDriver, defaultWindowHandle);
            if(result.Succeeded == false)
            {
                _logger.LogError("Failed to switch back to default tab in ProspectList browser");
                return result;
            };

            result.Value = (T)primaryProspectsPayload;
            return result;
        }

        #endregion

        private IList<PrimaryProspectRequest> CollectProspects(IWebDriver webDriver, string searchUrl, int totalResults, string primaryProspectListId)            
        {
            List<string> windowHandles = new();
            IList<PrimaryProspectRequest> prospects = new List<PrimaryProspectRequest>();

            try
            {
                for (int i = 0; i < totalResults; i++)
                {
                    if(i == 0)
                    {
                        HalOperationResult<INewTabOperation> newTabOperation = _webDriverProvider.NewTab<INewTabOperation>(webDriver);
                        if (newTabOperation.Succeeded == false)
                        {
                            break;
                        }

                        windowHandles.Add(newTabOperation.Value.WindowHandleId);

                        HalOperationResult<IOperationResponse> goToPageResult = GoToPage<IOperationResponse>(webDriver, searchUrl);
                        if (goToPageResult.Succeeded == false)
                        {
                            break;
                        }
                    }

                    bool isNoSearchResultsContainerDisplayed = _linkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
                    if(isNoSearchResultsContainerDisplayed == true)
                    {
                        HalOperationResult<IOperationResponse> retrySearchResult = _linkedInSearchPage.ClickRetrySearch<IOperationResponse>(webDriver);
                        if(retrySearchResult.Succeeded == false)
                        {
                            break;
                        }
                    }

                    HalOperationResult<IGatherProspects> result = _linkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
                    if (result.Succeeded == false)
                    {
                        continue;
                    }

                    List<IWebElement> propsAsWebElements = result.Value.ProspectElements;
                    prospects = prospects.Concat(CreatePrimaryProspects(propsAsWebElements, primaryProspectListId)).ToList();

                    HalOperationResult<IOperationResponse> clickNextResult = _linkedInSearchPage.ClickNext<IOperationResponse>(webDriver);
                    if(clickNextResult.Succeeded == false)
                    {
                        _logger.LogError("Failed to navigate to the next page");
                        break;
                    }

                    if (i == 60)
                        break;
                }
            }
            finally
            {
                // close all opened tabs
                windowHandles.ForEach(windowHandle => _webDriverProvider.CloseTab<IOperationResponse>(BrowserPurpose.ProspectList, windowHandle));
            }

            return prospects;
        }

        private IList<PrimaryProspectRequest> CreatePrimaryProspects(List<IWebElement> prospects, string primaryProspectListId)
        {
            IList<PrimaryProspectRequest> primaryProspects = new List<PrimaryProspectRequest>();

            foreach (IWebElement webElement in prospects)
            {
                primaryProspects.Add(new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = GetProspectsName(webElement),
                    ProfileUrl = GetProspectsProfileUrl(webElement),
                    SearchResultAvatarUrl = GetProspectsSearchResultAvatarUrl(webElement),
                    Area = GetProspectsArea(webElement),
                    PrimaryProspectListId = primaryProspectListId,
                    EmploymentInfo = GetProspectsEmploymentInfo(webElement)
                });
            }

            return primaryProspects;
        }

        #region Extract Prospect Details

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
                catch(Exception ex)
                {
                    // ignore the error and proceed
                    prospectName = prospectNameSpan.Text;
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects name");
            }

            return prospectName;
        }
        private string GetProspectsProfileUrl(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            string userName = innerText[0] ?? string.Empty;
            if(userName == "LinkedIn Member")
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
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects profile url");
            }            

            return profileUrl;
        }
        private string GetProspectsSearchResultAvatarUrl(IWebElement webElement)
        {
            string avatarSrc = string.Empty;
            try
            {
                IWebElement img = webElement.FindElement(By.CssSelector(".presence-entity")).FindElement(By.TagName("img"));
                avatarSrc = img.GetAttribute("src");
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects avatar url");
            }
            return avatarSrc;
        }
        private string GetProspectsArea(IWebElement webElement)
        {
            string prospectArea = string.Empty;
            try
            {
                IWebElement secondarySubTitleElement = webElement.FindElement(By.CssSelector(".entity-result__secondary-subtitle"));
                prospectArea = secondarySubTitleElement.Text;
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects area");
            }
            return prospectArea;
        }
        private string GetProspectsEmploymentInfo(IWebElement webElement)
        {
            string prospectEmploymentInfo = string.Empty;
            try
            {
                IWebElement prospectEmploymentPTag = webElement.FindElement(By.CssSelector(".entity-result__summary"));
                prospectEmploymentInfo = prospectEmploymentPTag.Text;
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects employment info");
            }

            return prospectEmploymentInfo;
        }

        #endregion
                
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
