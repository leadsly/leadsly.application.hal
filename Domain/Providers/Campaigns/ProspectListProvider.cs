using Domain.POMs;
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
            ILinkedInMyNetworkPage linkedInMyNetworkPage,
            ILogger<ProspectListProvider> logger,
            IWebDriverProvider webDriverProvider,
            IHalIdentity halIdentity,
            ILinkedInNavBar linkedInNavBar,
            ICampaignPhaseProcessingService campaignProcessingPhase,
            ILinkedInHtmlParser linkedInHtmlParser,
            ILinkedInSearchPage linkedInSearchPage,
            IHalOperationConfigurationProvider halConfigurationProvider)
        {
            _logger = logger;
            _linkedInMyNetworkPage = linkedInMyNetworkPage;
            _campaignProcessingPhase = campaignProcessingPhase;
            _linkedInSearchPage = linkedInSearchPage;
            _webDriverProvider = webDriverProvider;
            _linkedInNavBar = linkedInNavBar;
            _linkedInHomePage = linkedInHomePage;
            _linkedInHtmlParser = linkedInHtmlParser;
            _halConfigurationProvider = halConfigurationProvider;
            _halIdentity = halIdentity;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInNavBar _linkedInNavBar;
        private readonly ILogger<ProspectListProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInHtmlParser _linkedInHtmlParser;
        private readonly ILinkedInMyNetworkPage _linkedInMyNetworkPage;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHalOperationConfigurationProvider _halConfigurationProvider;
        private readonly IHalIdentity _halIdentity;

        #region Execute ProspectList Phase

        public async Task<HalOperationResult<T>> ExecutePhase<T>(ProspectListBody message) 
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
            HalOperationResult<T> result = new();
            IEnumerable<PrimaryProspect> prospects = new List<PrimaryProspect>();
            foreach (string searchUrl in message.SearchUrls)
            {
                result = GoToPage<T>(webDriver, searchUrl);
                if (result.Succeeded == false)
                {
                    result.Failures.Add(new()
                    {
                        Code = Codes.WEBDRIVER_ERROR,
                        Reason = "Failed to navigate to the given page",
                        Detail = $"Failed to go to page {searchUrl}"
                    });
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

                IEnumerable<PrimaryProspect> primaryProspects = CollectProspects(webDriver, searchUrl, totalResults, message.PrimaryProspectListId);

                if (primaryProspects != null)
                {
                    prospects = prospects.Concat(primaryProspects);
                }
            }

            IPrimaryProspectListPayload primaryProspectsPayload = new PrimaryProspectListPayload
            {
                Prospects = prospects
            };

            result.Value = (T)primaryProspectsPayload;
            return result;
        }

        #endregion

        private IEnumerable<PrimaryProspect> CollectProspects(IWebDriver webDriver, string searchUrl, int totalResults, string primaryProspectListId)            
        {
            string queryParam = "&page={num}";
            string nextPageUrl = searchUrl;
            List<IWebElement> prospectsAsWebElements = new();            
            List<string> windowHandles = new();
            for (int i = 1; i < totalResults; i++)
            {
                nextPageUrl += queryParam.Replace("{num}", i.ToString());

                HalOperationResult<INewTabOperation> newTabOperation = _webDriverProvider.NewTab<INewTabOperation>(webDriver);
                if(newTabOperation.Succeeded == false)
                {
                    continue;
                }
                windowHandles.Add(newTabOperation.Value.WindowHandleId);

                HalOperationResult<IOperationResponse> goToPageResult = GoToPage<IOperationResponse>(webDriver, nextPageUrl);
                if(goToPageResult.Succeeded == false)
                {
                    continue;
                }

                HalOperationResult<IGatherProspects> result = _linkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
                if(result.Succeeded == false) 
                {
                    continue;
                }

                List<IWebElement> propsAsWebElements = result.Value.ProspectElements;
                prospectsAsWebElements.AddRange(propsAsWebElements);

                if (i == 1)
                    break;
            }

            IEnumerable<PrimaryProspect> prospects = CreatePrimaryProspects(prospectsAsWebElements, primaryProspectListId);

            // close all opened tabs
            windowHandles.ForEach(windowHandle => _webDriverProvider.CloseTab<IOperationResponse>(BrowserPurpose.ProspectList, windowHandle));

            return prospects;
        }

        private IEnumerable<PrimaryProspect> CreatePrimaryProspects(List<IWebElement> prospects, string primaryProspectListId)
        {
            IList<PrimaryProspect> primaryProspects = new List<PrimaryProspect>();

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
                prospectName = prospectNameSpan.Text;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Webdriver error occured extracting prospects name");
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
                _logger.LogError(ex, "Webdriver error occured extracting prospects profile url");
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
                _logger.LogError(ex, "Webdriver error occured extracting prospects avatar url");
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
                _logger.LogError(ex, "Webdriver error occured extracting prospects area");
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
                _logger.LogError(ex, "Webdriver error occured extracting prospects employment info");
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
