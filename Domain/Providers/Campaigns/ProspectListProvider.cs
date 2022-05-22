using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.interfaces;
using Leadsly.Application.Model.Campaigns.ProspectList;
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
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ProspectListProvider : IProspectListProvider
    {
        public ProspectListProvider(
            ILogger<ProspectListProvider> logger,
            IWebDriverProvider webDriverProvider,
            ILinkedInPageFacade linkedInPageFacade,
            IHumanBehaviorService humanBehaviorService,
            IPhaseDataProcessingProvider phaseDataProcessingProvider
            )
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
            _webDriverProvider = webDriverProvider;
            _linkedInPageFacade = linkedInPageFacade;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IPhaseDataProcessingProvider _phaseDataProcessingProvider;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<ProspectListProvider> _logger;
        private readonly ILinkedInPageFacade _linkedInPageFacade;        

        #region Execute ProspectList Phase

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ProspectListBody message)
            where T : IOperationResponse
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing ProspectList Phase on hal id {halId}", halId);

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

            result = await ProspectListAsync<T>(webDriver, message);

            _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.ProspectList);

            return result;
        }
        private async Task<HalOperationResult<T>> ProspectListAsync<T>(IWebDriver webDriver, ProspectListBody message)
            where T : IOperationResponse
        {
            string defaultWindowHandle = webDriver.CurrentWindowHandle;
            _logger.LogDebug("Current window handle is: {defaultWindowHandle}", defaultWindowHandle);

            HalOperationResult<T> result = new();
            IList<PrimaryProspectRequest> prospects = new List<PrimaryProspectRequest>();
            foreach (string searchUrl in message.SearchUrls)
            {
                result = GoToPage<T>(webDriver, searchUrl);
                if (result.Succeeded == false)
                {
                    return result;
                }

                result = _linkedInPageFacade.LinkedInSearchPage.GetTotalSearchResults<T>(webDriver);
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
                _logger.LogDebug("Total results in the hitlist {totalResults}", totalResults);

                await CollectProspectsAsync(webDriver, searchUrl, totalResults, message);

            }

            result.Succeeded = true;
            return result;
        }

        #endregion

        private async Task CollectProspectsAsync(IWebDriver webDriver, string searchUrl, int totalResults, ProspectListBody message)
        {
            _logger.LogInformation("Starting to collect all of the prospects from search url {searchUrl}." +
                "\r\n Total results for the search results are: {totalResults} " +
                "\r\n Primary prospect list id is {primaryProspectListId}", searchUrl, totalResults, message.PrimaryProspectListId);

            for (int i = 0; i < totalResults; i++)
            {
                bool crawlResult = CrawlProspects(webDriver, message.PrimaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects);
                if(crawlResult == false)
                {
                    break;
                }

                HalOperationResult<IOperationResponse> result = await _phaseDataProcessingProvider.ProcessProspectListAsync<IOperationResponse>(collectedProspects, message);
                if(result.Succeeded == false)
                {
                    _logger.LogError("Failed to process scraped prospect list. This was batch {i} out of {totalResults}", i, totalResults);
                }
            }
        }

        private bool CrawlProspects(IWebDriver webDriver, string primaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects)
        {
            bool crawlResult = false;
            collectedProspects = new List<PrimaryProspectRequest>();

            bool isNoSearchResultsContainerDisplayed = _linkedInPageFacade.LinkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
            if (isNoSearchResultsContainerDisplayed == true)
            {
                HalOperationResult<IOperationResponse> retrySearchResult = _linkedInPageFacade.LinkedInSearchPage.ClickRetrySearch<IOperationResponse>(webDriver);
                if (retrySearchResult.Succeeded == false)
                {
                    return crawlResult;
                }
            }

            _humanBehaviorService.RandomWaitMilliSeconds(2000, 7000);
            IWebElement resultsDiv = _linkedInPageFacade.LinkedInSearchPage.ResultsHeader(webDriver);
            _humanBehaviorService.RandomClickElement(resultsDiv);

            HalOperationResult<IGatherProspects> result = _linkedInPageFacade.LinkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
            // ify on this, perhpas this should just return false
            if (result.Succeeded == false)
            {
                crawlResult = true;
                return crawlResult;
            }

            List<IWebElement> propsAsWebElements = result.Value.ProspectElements;

            // we need to perform a random scroll
            if (propsAsWebElements.Count > 3)
            {
                _linkedInPageFacade.LinkedInSearchPage.ScrollIntoView(propsAsWebElements[2], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            if (propsAsWebElements.Count > 8)
            {
                _linkedInPageFacade.LinkedInSearchPage.ScrollIntoView(propsAsWebElements[7], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            _linkedInPageFacade.LinkedInSearchPage.ScrollIntoView(propsAsWebElements[3], webDriver);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 3000);

            _logger.LogTrace("Creating PrimaryProspects from IWebElements");
            collectedProspects = CreatePrimaryProspects(propsAsWebElements, primaryProspectListId);

            IWebElement areResultsHelpfulText = _linkedInPageFacade.LinkedInSearchPage.AreResultsHelpfulPTag(webDriver);
            _humanBehaviorService.RandomClickElement(areResultsHelpfulText);

            HalOperationResult<IOperationResponse> scrollFooterIntoViewResult = _linkedInPageFacade.LinkedInSearchPage.ScrollFooterIntoView<IOperationResponse>(webDriver);
            if (scrollFooterIntoViewResult.Succeeded == false)
            {
                _logger.LogError("Failed to scroll footer into view");
                return crawlResult;
            }

            IWebElement linkedInFooterLogo = _linkedInPageFacade.LinkedInSearchPage.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInFooterLogo);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 5000);

            HalOperationResult<IOperationResponse> clickNextResult = _linkedInPageFacade.LinkedInSearchPage.ClickNext<IOperationResponse>(webDriver);
            if (clickNextResult.Succeeded == false)
            {
                _logger.LogError("Failed to navigate to the next page");
                return crawlResult;
            }

            HalOperationResult<IOperationResponse> waitForResultsOperation = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<IOperationResponse>(webDriver);
            if (waitForResultsOperation.Succeeded == false)
            {
                _logger.LogError("Search results never finished loading.");
                return crawlResult;                
            }

            crawlResult = true;
            return crawlResult;
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
        private string GetProspectsSearchResultAvatarUrl(IWebElement webElement)
        {
            string avatarSrc = string.Empty;
            try
            {
                IWebElement img = webElement.FindElement(By.CssSelector(".presence-entity")).FindElement(By.TagName("img"));
                avatarSrc = img.GetAttribute("src");
            }
            catch (Exception ex)
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
            catch (Exception ex)
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
            catch (Exception ex)
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
