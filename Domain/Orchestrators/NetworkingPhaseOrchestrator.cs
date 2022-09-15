using Domain.Facades.Interfaces;
using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.SearchResultsLimit;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces.POMs;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Orchestrators
{
    public class NetworkingPhaseOrchestrator : PhaseOrchestratorBase, INetworkingPhaseOrchestrator
    {
        public NetworkingPhaseOrchestrator(
            ILogger<NetworkingPhaseOrchestrator> logger,
            INetworkingInteractionFacade interactionFacade,
            ISearchPageFooterServicePOM searchPageFooterService,
            IWebDriverProvider webDriverProvider
            ) : base(logger)
        {

            _searchPageFooterService = searchPageFooterService;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _interactionFacade = interactionFacade;
        }

        private readonly INetworkingInteractionFacade _interactionFacade;
        private readonly ISearchPageFooterServicePOM _searchPageFooterService;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<NetworkingPhaseOrchestrator> _logger;

        private List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; set; } = new List<PersistPrimaryProspectModel>();
        private IList<ConnectionSentModel> ConnectionsSent { get; set; } = new List<ConnectionSentModel>();
        private IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; set; } = new List<SearchUrlProgressModel>();
        private IList<IWebElement> Prospects { get; set; } = new List<IWebElement>();
        private int NumberOfConnectionsSent { get; set; }
        private bool MonthlySearchLimitReached;

        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls()
        {
            IList<SearchUrlProgressModel> searchUrlProgress = UpdatedSearchUrlsProgress;
            UpdatedSearchUrlsProgress = new List<SearchUrlProgressModel>();
            return searchUrlProgress;
        }

        public List<PersistPrimaryProspectModel> GetPersistPrimaryProspects()
        {
            List<PersistPrimaryProspectModel> prospects = PersistPrimaryProspects;
            PersistPrimaryProspects = new List<PersistPrimaryProspectModel>();
            return prospects;
        }

        public IList<ConnectionSentModel> GetConnectionsSent()
        {
            IList<ConnectionSentModel> connectionsSent = ConnectionsSent;
            ConnectionsSent = new List<ConnectionSentModel>();
            return connectionsSent;
        }

        public bool GetMonthlySearchLimitReached()
        {
            bool limitReached = MonthlySearchLimitReached;
            MonthlySearchLimitReached = false;
            return limitReached;
        }

        public void Execute(NetworkingMessageBody message, IList<SearchUrlProgressModel> searchUrlsProgress)
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing Networking Phase on hal id {halId}", halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.Networking, message.ChromeProfileName, message.GridNamespaceName, message.GridServiceDiscoveryName, out bool isNewWebDriver);
            if (webDriver == null)
            {
                _logger.LogError("WebDriver could not be found or created. Cannot proceed");
                return;
            }

            ExecuteInternal(message, webDriver, searchUrlsProgress);
        }

        private void ExecuteInternal(NetworkingMessageBody message, IWebDriver webDriver, IList<SearchUrlProgressModel> searchUrlsProgress)
        {
            try
            {
                BeginNetworking(message, webDriver, searchUrlsProgress);
            }
            finally
            {
                _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Networking);
                NumberOfConnectionsSent = 0;
            }
        }

        private void BeginNetworking(NetworkingMessageBody message, IWebDriver webDriver, IList<SearchUrlProgressModel> searchUrlsProgress)
        {
            _logger.LogDebug("Begning to execute networking phase");
            foreach (SearchUrlProgressModel searchUrlProgress in searchUrlsProgress)
            {
                if (PrepareBrowser(webDriver, searchUrlProgress, message.HalId) == false)
                {
                    return;
                }

                int? totalResults = GetTotalNumberOfSearchResults(webDriver, searchUrlProgress);
                if (totalResults == null)
                {
                    // just move onto the next search url
                    _logger.LogDebug($"Unable to determine total number of search results. Moving on to the next search url in the list. This failure occured for search result: {searchUrlProgress.SearchUrl}");
                    continue;
                }

                Add_UpdateSearchUrlProgressRequest(searchUrlProgress.SearchUrlProgressId, searchUrlProgress.LastPage, webDriver.Url, (int)totalResults, webDriver.CurrentWindowHandle);

                ConnectWithProspectsForSearchUrl(webDriver, message, searchUrlProgress, (int)totalResults);

                // see if we can get away with this.
                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                    break;

                if (MonthlySearchLimitReached == true)
                    break;
            }
            _logger.LogDebug("Finished executing networking phase");
        }

        private void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults)
        {
            _logger.LogInformation("ConnectWithProspectsForSearchUrl executing");
            for (int currentPage = searchUrlProgress.LastPage; currentPage < totalResults + 1; currentPage++)
            {
                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);

                if (GatherProspectsInteractions(webDriver, message) == false)
                {
                    _logger.LogDebug("Gathering prospects on the search results page failed. Checking if next button is disabled");
                    if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                    {
                        _logger.LogDebug("Next button is disabled. Checking if this is the last page");
                        // if next button is disabled check if we are on the last page
                        if (IsLastPage(webDriver, currentPage, totalResults) == true)
                        {
                            _logger.LogDebug("This is the last page. Updating SearchUrlProgress request and breaking out of the loop.");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }
                        else
                        {
                            _logger.LogDebug("This is not the last page. Chcking if search result monthly limit has been reached.");
                            string currentUrl = webDriver.Url;
                            if (IsMonthlySearchLimitReached(webDriver) == true)
                            {
                                _logger.LogDebug("Monthl search limit has been reached. Setting monthly search limit request and breaking.");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                                SetMonthlyLimitReachedRequest(true);
                                break;
                            }
                            else
                            {
                                // something simply went wrong we could retry or move on to the next page
                                _logger.LogDebug("Monthyl search limit was not reached. Something appears to have gone wrong, updating SearchUrlProgress and breaking.");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                break;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Next button is not disabled. Trying to move onto the next page.");
                        if (GoToTheNextPage(webDriver, searchUrlProgress.SearchUrlProgressId, currentPage, totalResults) == false)
                        {
                            _logger.LogDebug("Failed to navigate to the next search results page. Updating SearchUrlProgress request and breaking.");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }
                    }

                    continue;
                }

                _logger.LogInformation("Adding PrimaryProspectRequests List");
                PersistPrimaryProspects.AddRange(_interactionFacade.PersistPrimaryProspects);

                // if number of prospects comes back as zero continue to the next page
                if (_interactionFacade.Prospects.Count == 0)
                {
                    _logger.LogDebug("There aren't any prospects on this page that we can connect with. Checking if this is the last page.");
                    if (IsLastPage(webDriver, currentPage, totalResults))
                    {
                        _logger.LogDebug("This is the last page of this search url. Updating SearchUrlProgress and breaking");
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, true);
                        break;
                    }
                    _logger.LogDebug("This is not the last page of this search url. Checking if next button is disabled");

                    if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                    {
                        _logger.LogDebug("The next button of the search url is disabled. Checking if we have reached the monthly search limit");
                        string currentUrl = webDriver.Url;
                        if (IsMonthlySearchLimitReached(webDriver) == true)
                        {
                            _logger.LogDebug("Monthly search limit has been reached. Setting MonthlySearchLimitReached request and breaking");
                            SetMonthlyLimitReachedRequest(true);
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                            break;
                        }

                        _logger.LogDebug("Next button is disabled and we have not reached monthly search limit. Something went wrong. Updating SearchUrlProgress request and breaking");
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                        break;
                    }

                    _logger.LogDebug("The next button of the search url is not disabled. Navigating to the next page.");
                    if (GoToTheNextPage(webDriver, searchUrlProgress.SearchUrlProgressId, currentPage, totalResults) == false)
                    {
                        _logger.LogDebug("Failed to navigate to the next search url page.");
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                        break;
                    }

                    continue;
                }
                else
                {
                    Prospects = _interactionFacade.Prospects;
                }

                // if there are available prospects connect with them
                ConnectWithProspectsOnCurrentPage(webDriver, message, currentPage, totalResults, searchUrlProgress.SearchUrlProgressId);

                if (IsMaxNumberOfConnectionsSentReached(message.ProspectsToCrawl))
                {
                    _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", NumberOfConnectionsSent, message.ProspectsToCrawl);
                    if (IsLastPage(webDriver, currentPage, totalResults))
                    {
                        // if number of available prospects on this page, matches the number of connection requests we have sent, we've exhausted this url
                        bool exhausted = Prospects.Count == ConnectionsSent.Count;
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, exhausted);
                    }
                    else
                    {
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                    }

                    break;
                }

                // if this is the last page update search url progress request with exhausted property set to true                
                if (IsLastPage(webDriver, currentPage, totalResults))
                {
                    _logger.LogDebug("We are on the last page for this search result");
                    bool exhausted = Prospects.Count == ConnectionsSent.Count;
                    _logger.LogDebug("Has this search url been exhausted: {0}", exhausted);
                    UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, exhausted);
                    break;
                }
                else
                {
                    _logger.LogDebug("We are not on the last page. Checking if next button is disabled.");
                    if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                    {
                        _logger.LogDebug("Next button is disabled. Checking if we have reached our monthly search limit");
                        string currentUrl = webDriver.Url;
                        if (IsMonthlySearchLimitReached(webDriver) == true)
                        {
                            _logger.LogDebug("Monthly search limit has been reached. Setting MonthlyLimitReached request and updating SearchUrlProgress");
                            SetMonthlyLimitReachedRequest(true);
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                            break;
                        }
                        else
                        {
                            _logger.LogDebug("Monthly search limit has not been reached. Something went wrong and we can't continue to the next page. Updating SearchUrlProgress and breaking");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Next button is not disabled. Navigating to the next page.");
                        // if this is not the last page go to the next page
                        if (GoToTheNextPage(webDriver, searchUrlProgress.SearchUrlProgressId, currentPage, totalResults) == false)
                        {
                            _logger.LogDebug("Failed to navigate to the next page. Updating SearchUrlProgress and breaking.");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }
                    }
                }
            }
        }

        private void SetMonthlyLimitReachedRequest(bool limitReached)
        {
            MonthlySearchLimitReached = limitReached;
        }

        private bool IsMonthlySearchLimitReached(IWebDriver webDriver)
        {
            SearchResultsLimitInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleSearchResultsLimitInteraction(interaction);
        }

        private bool GatherProspectsInteractions(IWebDriver webDriver, NetworkingMessageBody message)
        {
            GatherProspectsInteraction gatherProspectsInteraction = new()
            {
                WebDriver = webDriver,
                Message = message,
            };

            return _interactionFacade.HandleGatherProspectsInteraction(gatherProspectsInteraction);
        }

        private void ConnectWithProspectsOnCurrentPage(IWebDriver webDriver, NetworkingMessageBody message, int currentPage, int totalResults, string searchUrlProgressId)
        {
            _logger.LogTrace("Executing ConnectWithProspectsOnCurrentPage");
            _logger.LogDebug("Number of connectable prospects: {0}", Prospects?.Count);
            ConnectWithProspectInteraction interaction = new()
            {
                CurrentPage = currentPage,
                TotalResults = totalResults,
                Message = message,
                WebDriver = webDriver
            };

            foreach (IWebElement prospect in Prospects)
            {
                if (IsMaxNumberOfConnectionsSentReached(message.ProspectsToCrawl))
                {
                    _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", NumberOfConnectionsSent, message.ProspectsToCrawl);
                    break;
                }

                interaction.Prospect = prospect;
                bool succeeded = _interactionFacade.HandleConnectWithProspectsInteraction(interaction);
                if (succeeded == true)
                {
                    NumberOfConnectionsSent += 1;
                    ConnectionsSent.Add(_interactionFacade.ConnectionSent);
                }
                else
                {
                    _logger.LogDebug("The CampaignProspectRequest is null. Skipping this prospect and moving onto the next one");
                }
            }
        }

        private bool PrepareBrowser(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress, string halId)
        {
            HalOperationResult<IOperationResponse> result = _webDriverProvider.SwitchToOrNewTab<IOperationResponse>(webDriver, searchUrlProgress.WindowHandleId);
            if (result.Succeeded == false)
            {
                _logger.LogError("Failed to switch to an existing tab or create a new one. Hal id {halId}", halId);
                return false;
            }

            bool pageNavigationSucceeded = GoToPage(webDriver, searchUrlProgress.SearchUrl);
            if (pageNavigationSucceeded == false)
            {
                _logger.LogError("Failed to navigate to page {searchUrl}. Hal id {halId}", searchUrlProgress.SearchUrl, halId);
                return false;
            }

            if (NoSearchResultsDisplayed(webDriver) == true)
            {
                _logger.LogError("No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.");
                return false;
            }

            return true;
        }

        private bool NoSearchResultsDisplayed(IWebDriver webDriver)
        {
            NoResultsFoundInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            if (_interactionFacade.HandleNoResultsFoundInteraction(interaction))
            {
                _logger.LogError("No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.");
                return true;
            }
            return false;
        }

        private bool IsLastPage(IWebDriver webDriver, int currentPage, int totalResults)
        {
            if (_searchPageFooterService.IsLastPage(webDriver, false, currentPage, totalResults) == true)
            {
                return true;
            }
            return false;
        }

        private int? GetTotalNumberOfSearchResults(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress)
        {
            int totalResults = 0;
            if (searchUrlProgress.TotalSearchResults == 0)
            {
                int? totalNumberOfResults = _searchPageFooterService.GetTotalResults(webDriver, true);
                if (totalNumberOfResults == null)
                {
                    return null;
                }
                totalResults = (int)totalNumberOfResults;
            }
            else
            {
                totalResults = searchUrlProgress.TotalSearchResults;
            }

            return totalResults;
        }

        private bool IsMaxNumberOfConnectionsSentReached(int numberOfProspectsToCrawl)
        {
            if (NumberOfConnectionsSent >= numberOfProspectsToCrawl)
            {
                _logger.LogDebug("Number of connections sent has reached the max number of connections that can be sent");
                return true;
            }
            _logger.LogDebug("Number of connections sent has not reached the max number of connections that can be sent");

            return false;
        }

        private void UpdateCurrentPage_SearchUrlProgress(string searchUrlProgressId, int currentPage, string currentUrl, bool exhausted = false)
        {
            SearchUrlProgressModel searchUrl = UpdatedSearchUrlsProgress.FirstOrDefault(r => r.SearchUrlProgressId == searchUrlProgressId);
            if (searchUrl != null)
            {
                searchUrl.SearchUrl = currentUrl;
                searchUrl.LastPage = currentPage;
                searchUrl.Exhausted = exhausted;
            }
        }

        private void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle)
        {
            if (UpdatedSearchUrlsProgress.Any(req => req.SearchUrlProgressId == searchUrlProgressId) == false)
            {
                UpdatedSearchUrlsProgress.Add(new()
                {
                    SearchUrlProgressId = searchUrlProgressId,
                    StartedCrawling = true,
                    LastPage = currentPage,
                    SearchUrl = currentUrl,
                    TotalSearchResults = totalResults,
                    WindowHandleId = currentWindowHandle
                });
            }
        }

        private bool GoToTheNextPage(IWebDriver webDriver, string searchUrlProgressId, int currentPage, int totalResults)
        {
            bool succeeded = false;
            if (NavigateToNextPage(webDriver) == false)
            {
                succeeded = false;
            }
            else
            {
                succeeded = true;
            }

            return succeeded;
        }

        private bool NavigateToNextPage(IWebDriver webDriver)
        {
            bool succeeded = false;
            bool? goToNextPageOperation = _searchPageFooterService.GoToTheNextPage(webDriver);
            if (goToNextPageOperation == null || goToNextPageOperation == false)
            {
                _logger.LogDebug("Navigation to the next page failed. Breaking out of this loop and updating search page url progress");
                succeeded = false;
            }
            else
            {
                if (NoSearchResultsDisplayed(webDriver) == true)
                {
                    succeeded = false;
                }
                else
                {
                    succeeded = true;
                }
            }

            return succeeded;
        }

        private bool CheckIfNextButtonIsNullOrDisabled(IWebDriver webDriver)
        {
            bool disabled = false;
            bool? isNextButtonClickable = _searchPageFooterService.IsNextButtonClickable(webDriver);
            if (isNextButtonClickable == null)
            {
                _logger.LogInformation("Next button is null. Could not locate it. Checking if it is the last page");
                disabled = true;
            }
            else if (isNextButtonClickable == false)
            {
                _logger.LogInformation("Next button is disabled. Checking if it is the last page");
                disabled = true;
            }
            else
            {
                _logger.LogDebug("Next button is not disabled");
                disabled = false;
            }

            return disabled;
        }
    }
}
