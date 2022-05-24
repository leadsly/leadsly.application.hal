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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class NetworkingProvider : INetworkingProvider
    {
        public NetworkingProvider(
            ILogger<ProspectListProvider> logger,
            ICrawlProspectsService crawlProspectsService,
            IWebDriverProvider webDriverProvider,
            ICampaignProspectsService campaignProspectService,
            ILinkedInPageFacade linkedInPageFacade,
            IHumanBehaviorService humanBehaviorService,
            IPhaseDataProcessingProvider phaseDataProcessingProvider
            )
        {
            _logger = logger;
            _campaignProspectService = campaignProspectService;
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
        private readonly ICampaignProspectsService _campaignProspectService;
        private readonly ICrawlProspectsService _crawlProspectsService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;

        #region ProspectList

        public async Task<HalOperationResult<T>> ExecuteProspectListAsync<T>(NetworkingMessageBody message, SearchUrlProgress searchUrlProgress, CancellationToken ct = default) where T : IOperationResponse
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing ProspectList Phase on hal id {halId}", halId);

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.Networking,
                ChromeProfileName = message.ChromeProfileName,
                PageUrl = searchUrlProgress.SearchUrl
            };

            HalOperationResult<T> result = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (result.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            result = _webDriverProvider.SwitchToOrNewTab<T>(webDriver, searchUrlProgress.WindowHandleId);
            if(result.Succeeded == false)
            {
                return result;
            }

            result = await ProspectListAsync<T>(webDriver, message, searchUrlProgress);

            return result;
        }

        private async Task<HalOperationResult<T>> ProspectListAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgress searchUrlProgress)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = GoToPage<T>(webDriver, searchUrlProgress.SearchUrl);
            if (result.Succeeded == false)
            {
                return result;
            }

            // grab everyone from the search result hit list, determine how many prospects to process
            bool crawlResult = _crawlProspectsService.CrawlProspects(webDriver, message.PrimaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects);
            if (crawlResult == false)
            {
                return result;
            }

            result = await _phaseDataProcessingProvider.ProcessProspectListAsync<T>(collectedProspects, message, message.CampaignId, message.PrimaryProspectListId, message.CampaignProspectListId);
            if (result.Succeeded == false)
            {
                _logger.LogError("Failed to process scraped prospect list");
            }

            result.Succeeded = true;
            return result;
        }

        #endregion

        #region SendConnections

        public async Task<HalOperationResult<T>> ExecuteSendConnectionsAsync<T>(NetworkingMessageBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.Networking,
                ChromeProfileName = message.ChromeProfileName
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            result = await SendConnectionRequestsAsync<T>(message, webDriver);

            if(result.Succeeded == false)
            {
                return result;
            }

            ISendConnectionsPayload sendConnectionspayload = ((ISendConnectionsPayload)result.Value);
            return await _phaseDataProcessingProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(sendConnectionspayload.CampaignProspects, message, message.CampaignId);
        }

        private async Task<HalOperationResult<T>> SendConnectionRequestsAsync<T>(NetworkingMessageBody message, IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            result = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<T>(webDriver);
            if (result.Succeeded == false)
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

            IList<CampaignProspectRequest> campaignProspectRequests = new List<CampaignProspectRequest>();
            IList<IWebElement> connectWith = gatherProspectsResult.Value.ProspectElements.Skip(message.ProspectsToCrawl).ToList();
            foreach (IWebElement prospect in connectWith)
            {
                // send connection
                bool sendConnectionSuccess = SendConnection(webDriver, prospect);
                if (sendConnectionSuccess == false)
                {
                    result.Failures.Add(new()
                    {
                        Detail = "Failed to send connection request",
                        Reason = "Error occured when trying to send connection request"
                    });
                    return result;
                }

                CampaignProspectRequest request = _campaignProspectService.CreateCampaignProspects(prospect, message.CampaignId);
                campaignProspectRequests.Add(request);
            }

            result = await _phaseDataProcessingProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(campaignProspectRequests, message, message.CampaignId);
            if(result.Succeeded == false)
            {
                return result;
            }

            // TODO figure out how to update SearchUrlProgress
            
            result.Succeeded = true;
            return result;
        }

        private bool SendConnection(IWebDriver webDriver, IWebElement prospect)
        {
            bool sendConnectionSuccess = true;
            _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");

            IWebElement searchResultsHeader = _linkedInPageFacade.LinkedInSearchPage.ResultsHeader(webDriver);
            _humanBehaviorService.RandomClickElement(searchResultsHeader);

            _humanBehaviorService.RandomWaitMilliSeconds(700, 3000);
            HalOperationResult<IOperationResponse> sendConnectionResult = _linkedInPageFacade.LinkedInSearchPage.SendConnectionRequest<IOperationResponse>(prospect);
            if (sendConnectionResult.Succeeded == false)
            {
                sendConnectionSuccess = true;
            }

            IWebElement modalContent = _linkedInPageFacade.LinkedInSearchPage.GetCustomizeThisInvitationModalContent(webDriver);
            _humanBehaviorService.RandomClickElement(modalContent);

            _humanBehaviorService.RandomWaitMilliSeconds(700, 1400);
            HalOperationResult<IOperationResponse> clickSendConnectionResult = _linkedInPageFacade.LinkedInSearchPage.ClickSendInModal<IOperationResponse>(webDriver);
            if (clickSendConnectionResult.Succeeded == false)
            {
                sendConnectionSuccess = false;
            }

            return sendConnectionSuccess;
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
