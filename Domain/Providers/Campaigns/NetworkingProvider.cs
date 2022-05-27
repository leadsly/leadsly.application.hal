using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
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
            ICampaignProvider campaignProvider,
            ITimestampService timestampService,
            IHumanBehaviorService humanBehaviorService,
            IPhaseDataProcessingProvider phaseDataProcessingProvider
            )
        {
            _logger = logger;
            _campaignProvider = campaignProvider;
            _timestampService = timestampService;
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
        private readonly ICampaignProvider _campaignProvider;
        private readonly ITimestampService _timestampService;

        private int NumberOfConnectionsSent { get; set; }

        public async Task<HalOperationResult<T>> ExecuteNetworkingAsync<T>(NetworkingMessageBody message, IList<SearchUrlProgressRequest> searchUrlsProgress, CancellationToken ct = default) where T : IOperationResponse
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing Networking Phase on hal id {halId}", halId);

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.Networking,
                ChromeProfileName = message.ChromeProfileName
            };

            HalOperationResult<T> result = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (result.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            try
            {
                result = await ExecuteNetworkingInternalAsync<T>(message, webDriver, searchUrlsProgress, ct);
            }
            finally
            {
                NumberOfConnectionsSent = 0;
            }
            
            return result;
        }

        private async Task<HalOperationResult<T>> ExecuteNetworkingInternalAsync<T>(NetworkingMessageBody message, IWebDriver webDriver, IList<SearchUrlProgressRequest> searchUrlsProgress, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            foreach (SearchUrlProgressRequest searchUrlProgress in searchUrlsProgress)
            {
                result = _webDriverProvider.SwitchToOrNewTab<T>(webDriver, searchUrlProgress.WindowHandleId);
                if (result.Succeeded == false)
                {
                    return result;
                }

                result = GoToPage<T>(webDriver, searchUrlProgress.SearchUrl);
                if (result.Succeeded == false)
                {
                    return result;
                }

                result = await NetworkingAsync<T>(webDriver, message, searchUrlProgress);
                if(result.Succeeded == false)
                {
                    return result;
                }

                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                    break;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> GetTotalResults<T>(IWebDriver webDriver, SearchUrlProgressRequest searchUrlProgress)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // get total searchresults for this url
            _linkedInPageFacade.LinkedInSearchPage.ScrollFooterIntoView<T>(webDriver);
            _humanBehaviorService.RandomWaitSeconds(1, 2);

            // get the total number of search results
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

            result.Succeeded = true;
            return result;
        }


        private async Task<HalOperationResult<T>> NetworkingAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressRequest searchUrlProgress)
            where T : IOperationResponse
        {
            int totalResults = 0;
            if (searchUrlProgress.TotalSearchResults == 0)
            {
                HalOperationResult<T> result = GetTotalResults<T>(webDriver, searchUrlProgress);
                if (result.Succeeded == false)
                {
                    return result;
                }
                totalResults = ((IGetTotalNumberOfResults)result.Value).NumberOfResults;
            }
            else
            {
                totalResults = searchUrlProgress.TotalSearchResults;
            }

            return await ConnectWithProspectsAsync<T>(webDriver, message, searchUrlProgress, totalResults);
        }

        private async Task<HalOperationResult<T>> ConnectWithProspectsAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressRequest searchUrlProgress, int totalResults)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            int lastPage = searchUrlProgress.LastPage;
            for (int i = lastPage; i < totalResults; i++)
            {
                // get connactable prospects on this page
                IList<IWebElement> connectableProspects = GetConnectableProspects(webDriver, message.PrimaryProspectListId);
                if (connectableProspects == null)
                {
                    return result;
                }

                //if connectable prospects equals 0, go to the next page
                if(connectableProspects.Count == 0)
                {
                    result = _linkedInPageFacade.LinkedInSearchPage.ClickNext<T>(webDriver);
                    if (result.Succeeded == false)
                    {
                        _logger.LogError("Failed to navigate to the next page");
                        return result;
                    }

                    result = _linkedInPageFacade.LinkedInSearchPage.WaitUntilSearchResultsFinishedLoading<T>(webDriver);
                    if (result.Succeeded == false)
                    {
                        _logger.LogError("Search results never finished loading.");
                        return result;
                    }                    

                    if (i == totalResults)
                    {
                        result = await UpdateSearchUrlProgressAsync<T>(webDriver, searchUrlProgress, message, lastPage, true, totalResults);
                        break;
                    }

                    lastPage += 1;

                    continue;
                }

                // ProspectListPhase
                result = await ExecuteProspectListInternalAsync<T>(webDriver, message, connectableProspects);
                if(result.Succeeded == false)
                {
                    return result;
                }            

                // SendConnectionsPhase
                result = await ExecuteSendConnectionsInternalAsync<T>(webDriver, message, connectableProspects);
                if(result.Succeeded == false)
                {
                    return result;
                }

                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                {
                    result = await UpdateSearchUrlProgressAsync<T>(webDriver, searchUrlProgress, message, lastPage, false, totalResults);
                    break;
                }

                if (i == totalResults)
                {
                    result = await UpdateSearchUrlProgressAsync<T>(webDriver, searchUrlProgress, message, lastPage, true, totalResults);
                    break;
                }

                lastPage += 1;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<HalOperationResult<T>> UpdateSearchUrlProgressAsync<T>(IWebDriver webDriver, SearchUrlProgressRequest searchUrlProgress, NetworkingMessageBody message, int currentPage, bool markExhausted, int totalSearchResults)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
 
            string currentUrl = webDriver.Url;
            string currentWindowHandleId = webDriver.CurrentWindowHandle;

            searchUrlProgress.StartedCrawling = true;
            searchUrlProgress.Exhausted = markExhausted;
            searchUrlProgress.LastPage = currentPage;
            searchUrlProgress.SearchUrl = currentUrl;
            searchUrlProgress.TotalSearchResults = totalSearchResults;
            searchUrlProgress.WindowHandleId = currentWindowHandleId;
            searchUrlProgress.LastActivityTimestamp = _timestampService.TimestampNow();

            return await _campaignProvider.UpdateSearchUrlProgressAsync<T>(searchUrlProgress, message);
        }

        private async Task<HalOperationResult<T>> ExecuteSendConnectionsInternalAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, IList<IWebElement> connectableProspects)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            IList<CampaignProspectRequest> campaignProspectRequests = new List<CampaignProspectRequest>();
            foreach (IWebElement connectableProspect in connectableProspects)
            {   
                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                    break;

                CampaignProspectRequest request = ExecuteSendConnectionInternal(webDriver, connectableProspect, message.CampaignId);                
                campaignProspectRequests.Add(request);                
                NumberOfConnectionsSent += 1;
            }

            if(campaignProspectRequests.Count > 0)
            {
                result = await _phaseDataProcessingProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(campaignProspectRequests, message, message.CampaignId);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }
            result.Succeeded = true;
            return result;
        }

        private CampaignProspectRequest ExecuteSendConnectionInternal(IWebDriver webDriver, IWebElement prospect, string campaignId)
        {
            // send connection
            bool sendConnectionSuccess = SendConnection(webDriver, prospect);
            if (sendConnectionSuccess == false)
            {
                return null;
            }

            CampaignProspectRequest request = _campaignProspectService.CreateCampaignProspects(prospect, campaignId);
            return request;
        }

        private async Task<HalOperationResult<T>> ExecuteProspectListInternalAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, IList<IWebElement> connectableProspectsOnThisPage)
            where T : IOperationResponse
        {
            // save prospects in batches
            HalOperationResult<T> result = await PersistConnectableProspectsAsync<T>(connectableProspectsOnThisPage, message);

            if (result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<HalOperationResult<T>> PersistConnectableProspectsAsync<T>(IList<IWebElement> connectableProspectsOnThisPage, NetworkingMessageBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (connectableProspectsOnThisPage.Count > 0)
            {
                IList<PrimaryProspectRequest> collectedProspects = _crawlProspectsService.CreatePrimaryProspects(connectableProspectsOnThisPage, message.PrimaryProspectListId);
                result = await _phaseDataProcessingProvider.ProcessProspectListAsync<T>(collectedProspects, message, message.CampaignId, message.PrimaryProspectListId, message.CampaignProspectListId);
                if (result.Succeeded == false)
                {
                    // _logger.LogError("Failed to process scraped prospect list. This was batch {i} out of {totalResults}", i, totalResults);
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }

        /// <summary>
        /// Prospect is 'Connectable' if the user can send them a connection request.
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="primaryProspectListId"></param>
        /// <returns></returns>
        private IList<IWebElement> GetConnectableProspects(IWebDriver webDriver, string primaryProspectListId)
        {
            bool crawlResult = _crawlProspectsService.CrawlProspects(webDriver, primaryProspectListId, out IList<IWebElement> rawCollectedProspects);
            if (crawlResult == false)
            {
                return null;
            }
            // filter down the list to only those prospects that we can connect with
            IList<IWebElement> connectableProspectsOnThisPage = rawCollectedProspects.Where(r =>
            {
                IWebElement actionBtn = _linkedInPageFacade.LinkedInSearchPage.GetProspectsActionButton(r);
                if (actionBtn == null)
                {
                    return false;
                }
                return actionBtn.Text == ApiConstants.PageObjectConstants.Connect;
            }).ToList();

            return connectableProspectsOnThisPage;
        }

        private bool SendConnection(IWebDriver webDriver, IWebElement prospect)
        {
            bool sendConnectionSuccess = true;
            _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");

            _humanBehaviorService.RandomWaitMilliSeconds(700, 3000);
            _linkedInPageFacade.LinkedInSearchPage.ScrollTop(webDriver);
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
