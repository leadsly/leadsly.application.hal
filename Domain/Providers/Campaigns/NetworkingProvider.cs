using Domain.Facades.Interfaces;
using Domain.POMs.Dialogs;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Networking;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
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
            ISearchPageDialogManager searchPageDialogManager,
            ITimestampService timestampService,
            IHumanBehaviorService humanBehaviorService,
            IPhaseDataProcessingProvider phaseDataProcessingProvider,
            ISendConnectionsService sendConnectionsService
            )
        {
            _sendConnectionsService = sendConnectionsService;
            _logger = logger;
            _searchPageDialogManager = searchPageDialogManager;
            _campaignProvider = campaignProvider;
            _timestampService = timestampService;
            _campaignProspectService = campaignProspectService;
            _crawlProspectsService = crawlProspectsService;
            _humanBehaviorService = humanBehaviorService;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
            _webDriverProvider = webDriverProvider;
            _linkedInPageFacade = linkedInPageFacade;
        }

        private readonly ISendConnectionsService _sendConnectionsService;
        private readonly ISearchPageDialogManager _searchPageDialogManager;
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

        /// <summary>
        /// Connectable Prospects Query
        /// </summary>
        private Func<IWebElement, bool> ConnectableProspects
        {
            get
            {
                return (IWebElement r) =>
                {
                    IWebElement actionBtn = _linkedInPageFacade.LinkedInSearchPage.GetProspectsActionButton(r);
                    if (actionBtn == null)
                    {
                        return false;
                    }
                    return actionBtn.Text == ApiConstants.PageObjectConstants.Connect;
                };
            }
        }

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

            HalOperationResult<T> result = _webDriverProvider.GetOrCreateWebDriver<T>(operationData, message.GridNamespaceName, message.GridServiceDiscoveryName);
            if (result.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            try
            {
                await ExecuteNetworkingInternalAsync<T>(message, webDriver, searchUrlsProgress, ct);
            }
            finally
            {
                _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Networking);
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
                if (result.Succeeded == false)
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

            IWebElement linkedInlogoFooter = _linkedInPageFacade.LinkedInSearchPage.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInlogoFooter);
            _humanBehaviorService.RandomWaitMilliSeconds(500, 800);

            _linkedInPageFacade.LinkedInSearchPage.ScrollTop(webDriver);

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
            _logger.LogInformation("Executing ConnectWithProspectsAsync method");
            HalOperationResult<T> result = new();

            int lastPage = searchUrlProgress.LastPage;
            for (int i = lastPage; i < totalResults + 1; i++)
            {
                // get connactable prospects on this page
                IList<IWebElement> connectableProspects = GetConnectableProspects(webDriver, message.PrimaryProspectListId);
                if (connectableProspects == null)
                {
                    _logger.LogDebug("No connectable prospects have been found on this page");
                    return result;
                }

                //if connectable prospects equals 0, go to the next page
                if (connectableProspects.Count == 0)
                {
                    // if we are on the last page and there are no prospects to crawl update the search url
                    if (await IsLastPageAsync(lastPage, totalResults, webDriver, message, searchUrlProgress))
                    {
                        _logger.LogDebug("This is the last page and there are no prospects to crawl. Breaking out of the loop");
                        break;
                    }

                    result = GoToTheNextPage<T>(webDriver, totalResults, ref lastPage);
                    if (result.Succeeded == false)
                    {
                        _logger.LogDebug("Navigation to the next page failed.");
                        return result;
                    }

                    continue;
                }

                // ProspectListPhase
                result = await ExecuteProspectListInternalAsync<T>(webDriver, message, connectableProspects);
                if (result.Succeeded == false)
                {
                    return result;
                }

                // SendConnectionsPhase
                result = await ExecuteSendConnectionsInternalAsync<T>(webDriver, message, connectableProspects);
                if (result.Succeeded == false)
                {
                    return result;
                }

                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                {
                    result = await UpdateSearchUrlProgressAsync<T>(webDriver, searchUrlProgress, message, lastPage, false, totalResults);
                    break;
                }

                IList<IWebElement> afterOperationConnectableProspects = connectableProspects.Where(ConnectableProspects).ToList();
                connectableProspects = afterOperationConnectableProspects;
                if (connectableProspects.Count == 0)
                {
                    // if we are on the last page and there are no prospects to crawl update the search url
                    if (await IsLastPageAsync(lastPage, totalResults, webDriver, message, searchUrlProgress))
                    {
                        break;
                    }

                    result = GoToTheNextPage<T>(webDriver, totalResults, ref lastPage);
                    if (result.Succeeded == false)
                    {
                        return result;
                    }
                }
            }

            result.Succeeded = true;
            return result;
        }

        private async Task<bool> IsLastPageAsync(int lastPage, int totalResults, IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressRequest searchUrlProgress)
        {
            bool isLastPage = false;
            if (lastPage == totalResults)
            {
                HalOperationResult<IOperationResponse> result = await UpdateSearchUrlProgressAsync<IOperationResponse>(webDriver, searchUrlProgress, message, lastPage, true, totalResults);
                isLastPage = true;
            }
            _logger.LogDebug($"It was determined that this {(isLastPage ? "is" : "is not")} the last page.");
            return isLastPage;
        }

        private HalOperationResult<T> GoToTheNextPage<T>(IWebDriver webDriver, int totalResults, ref int lastPage)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            HalOperationResult<IOperationResponse> scrollFooterIntoViewResult = _linkedInPageFacade.LinkedInSearchPage.ScrollFooterIntoView<IOperationResponse>(webDriver);
            if (scrollFooterIntoViewResult.Succeeded == false)
            {
                _logger.LogError("Failed to scroll footer into view");
                return result;
            }

            IWebElement linkedInFooterLogo = _linkedInPageFacade.LinkedInSearchPage.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInFooterLogo);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 5000);

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

            lastPage += 1;

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
            _logger.LogTrace("Executing SendConnections phase");
            HalOperationResult<T> result = new();
            IList<CampaignProspectRequest> campaignProspectRequests = new List<CampaignProspectRequest>();
            _logger.LogDebug("Number of connectable prospects: {0}", connectableProspects?.Count);
            foreach (IWebElement connectableProspect in connectableProspects)
            {
                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                {
                    _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", NumberOfConnectionsSent, message.ProspectsToCrawl);
                    break;
                }

                CampaignProspectRequest request = ExecuteSendConnectionInternal(webDriver, connectableProspect, message.CampaignId);
                if (request != null)
                {
                    campaignProspectRequests.Add(request);
                    NumberOfConnectionsSent += 1;
                }
                else
                {
                    _logger.LogDebug("The CampaignProspectRequest is null. Skipping this prospect and moving onto the next one");
                }
            }

            if (campaignProspectRequests.Count > 0)
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
            _logger.LogTrace("Preparing to connect with prospect.");

            // send connection
            bool sendConnectionSuccess = SendConnection(webDriver, prospect);
            if (sendConnectionSuccess == false)
            {
                _logger.LogDebug("Sending connection to the given prospect failed.");
                // if there was a failure attempt to close modal dialog if it is open
                _humanBehaviorService.RandomWaitMilliSeconds(850, 2000);
                _searchPageDialogManager.TryCloseModal(webDriver);

                return null;
            }

            CampaignProspectRequest request = _campaignProspectService.CreateCampaignProspects(prospect, campaignId);
            return request;
        }

        private async Task<HalOperationResult<T>> ExecuteProspectListInternalAsync<T>(IWebDriver webDriver, NetworkingMessageBody message, IList<IWebElement> connectableProspectsOnThisPage)
            where T : IOperationResponse
        {
            _logger.LogTrace("Executing ProspectList phase. This means hal is only looking to gather prospects with which the user can connect with.");
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
                _logger.LogDebug($"Persisting {connectableProspectsOnThisPage.Count} prospects as part of the ProspectListPhase.");
                IList<PrimaryProspectRequest> collectedProspects = _crawlProspectsService.CreatePrimaryProspects(connectableProspectsOnThisPage, message.PrimaryProspectListId);
                result = await _phaseDataProcessingProvider.ProcessProspectListAsync<T>(collectedProspects, message, message.CampaignId, message.PrimaryProspectListId, message.CampaignProspectListId);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }
            else
            {
                _logger.LogDebug("No connectable prospects were found on this page.");
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
            _logger.LogTrace("Crawling all of prospects on this page");
            bool crawlResult = _crawlProspectsService.CrawlProspects(webDriver, primaryProspectListId, out IList<IWebElement> rawCollectedProspects);
            if (crawlResult == false)
            {
                _logger.LogError("Failed to crawl prospects on the current page. Returning explicit null");
                return null;
            }
            // filter down the list to only those prospects that we can connect with
            IList<IWebElement> connectableProspectsOnThisPage = rawCollectedProspects.Where(ConnectableProspects).ToList();

            return connectableProspectsOnThisPage;
        }

        private bool SendConnection(IWebDriver webDriver, IWebElement prospect)
        {
            bool sendConnectionSuccess = true;
            _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");

            _humanBehaviorService.RandomWaitMilliSeconds(700, 3000);
            HalOperationResult<IOperationResponse> sendConnectionResult = _linkedInPageFacade.LinkedInSearchPage.SendConnectionRequest<IOperationResponse>(prospect);
            if (sendConnectionResult.Succeeded == false)
            {
                _logger.LogDebug("Clicking 'Connect' button on the prospect failed");
                sendConnectionSuccess = false;
            }
            else
            {
                _humanBehaviorService.RandomWaitMilliSeconds(700, 1500);
                sendConnectionSuccess = _searchPageDialogManager.HandleConnectWithProspectModal(webDriver);
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
