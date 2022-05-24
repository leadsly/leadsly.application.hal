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
            ICrawlProspectsService crawlProspectsService,
            ILinkedInPageFacade linkedInPageFacade,
            IHumanBehaviorService humanBehaviorService,
            IPhaseDataProcessingProvider phaseDataProcessingProvider
            )
        {
            _logger = logger;
            _crawlProspectsService = crawlProspectsService;
            _humanBehaviorService = humanBehaviorService;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
            _webDriverProvider = webDriverProvider;
            _linkedInPageFacade = linkedInPageFacade;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IPhaseDataProcessingProvider _phaseDataProcessingProvider;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<ProspectListProvider> _logger;
        private readonly ICrawlProspectsService _crawlProspectsService;
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
                ChromeProfileName = message.ChromeProfileName,
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
            HalOperationResult<T> result = new();            
            foreach (string searchUrl in message.SearchUrls)
            {
                result = GoToPage<T>(webDriver, searchUrl);
                if (result.Succeeded == false)
                {
                    return result;
                }

                bool monthlySearchLimitReached = _linkedInPageFacade.LinkedInSearchPage.MonthlySearchLimitReached(webDriver);
                if (monthlySearchLimitReached == true)
                {
                    _logger.LogInformation("User's monthly search limit has been reached.");
                    await _phaseDataProcessingProvider.UpdateSocialAccountMonthlySearchLimitAsync<T>(message.SocialAccountId, message);
                    break;
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
                bool monthlySearchLimitReached = _linkedInPageFacade.LinkedInSearchPage.MonthlySearchLimitReached(webDriver);
                if(monthlySearchLimitReached == true)
                {
                    _logger.LogInformation("User's monthly search limit has been reached.");
                    await _phaseDataProcessingProvider.UpdateSocialAccountMonthlySearchLimitAsync<IOperationResponse>(message.SocialAccountId, message);
                    break;
                }

                bool crawlResult = CrawlProspects(webDriver, message.PrimaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects);
                if(crawlResult == false)
                {
                    break;
                }

                HalOperationResult<IOperationResponse> clickNextResult = _linkedInPageFacade.LinkedInSearchPage.ClickNext<IOperationResponse>(webDriver);
                if (clickNextResult.Succeeded == false)
                {
                    _logger.LogError("Failed to navigate to the next page");
                    break;
                }

                HalOperationResult<IOperationResponse> waitForResultsOperation = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<IOperationResponse>(webDriver);
                if (waitForResultsOperation.Succeeded == false)
                {
                    _logger.LogError("Search results never finished loading.");
                    break;
                }

                HalOperationResult<IOperationResponse> result = await _phaseDataProcessingProvider.ProcessProspectListAsync<IOperationResponse>(collectedProspects, message, message.CampaignId, message.PrimaryProspectListId, message.CampaignProspectListId);
                if(result.Succeeded == false)
                {
                    _logger.LogError("Failed to process scraped prospect list. This was batch {i} out of {totalResults}", i, totalResults);
                }
            }
        }

        private bool CrawlProspects(IWebDriver webDriver, string primaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects)
        {
            return _crawlProspectsService.CrawlProspects(webDriver, primaryProspectListId, out collectedProspects);            
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
