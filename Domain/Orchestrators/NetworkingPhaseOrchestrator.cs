using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.Networking.GetTotalSearchResults;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
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
            IWebDriverProvider webDriverProvider,
            IConnectWithProspectsForSearchUrlInstructionSet instructionSet
            ) : base(logger)
        {

            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _interactionFacade = interactionFacade;
            _instructionSet = instructionSet;
        }

        private readonly IConnectWithProspectsForSearchUrlInstructionSet _instructionSet;
        private readonly INetworkingInteractionFacade _interactionFacade;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<NetworkingPhaseOrchestrator> _logger;
        private IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress => _instructionSet.UpdatedSearchUrlsProgress;
        private bool MonthlySearchLimitReached => _instructionSet.MonthlySearchLimitReached;
        private int NumberOfConnectionsSent
        {
            get
            {
                return _instructionSet.NumberOfConnectionsSent;
            }
            set
            {
                _instructionSet.NumberOfConnectionsSent = value;
            }
        }
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects => _instructionSet.GetPersistPrimaryProspects();
        public IList<ConnectionSentModel> ConnectionsSent => _instructionSet.GetConnectionsSent();

        public IList<SearchUrlProgressModel> GetUpdatedSearchUrlsProgress()
        {
            return _instructionSet.GetUpdatedSearchUrls();
        }
        public bool GetMonthlySearchLimitReached()
        {
            return _instructionSet.GetMonthlySearchLimitReached();
        }

        public void Execute(NetworkingMessageBody message, IList<SearchUrlProgressModel> searchUrlsProgress)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(NetworkingMessageBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.Networking, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
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

                if (GetTotalnumberOfSearchResults(webDriver, searchUrlProgress) == false)
                {
                    // just move onto the next search url
                    _logger.LogDebug($"Unable to determine total number of search results. Moving on to the next search url in the list. This failure occured for search result: {searchUrlProgress.SearchUrl}");
                    continue;
                }

                int totalResults = _interactionFacade.TotalNumberOfSearchResults;
                Add_UpdateSearchUrlProgressRequest(searchUrlProgress.SearchUrlProgressId, searchUrlProgress.LastPage, webDriver.Url, totalResults, webDriver.CurrentWindowHandle);

                _instructionSet.ConnectWithProspectsForSearchUrl(webDriver, message, searchUrlProgress, (int)totalResults);

                // see if we can get away with this.
                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                    break;

                if (MonthlySearchLimitReached == true)
                    break;
            }
            _logger.LogDebug("Finished executing networking phase");
        }

        private bool PrepareBrowser(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress, string halId)
        {
            string messageTypeName = nameof(NetworkingMessageBody);
            HalOperationResult<IOperationResponse> result = _webDriverProvider.SwitchToOrNewTab<IOperationResponse>(webDriver, searchUrlProgress.WindowHandleId);
            if (result.Succeeded == false)
            {
                _logger.LogError("Execution of {0} failed. Failed to switch to an existing tab or create a new one. Hal id {1}", messageTypeName, halId);
                return false;
            }

            bool pageNavigationSucceeded = GoToPage(webDriver, searchUrlProgress.SearchUrl);
            if (pageNavigationSucceeded == false)
            {
                _logger.LogError("Execution of {0} failed. Failed to navigate to page {1}. Hal id {2}", messageTypeName, searchUrlProgress.SearchUrl, halId);
                return false;
            }

            if (NoSearchResultsDisplayed(webDriver) == true)
            {
                _logger.LogError("Execution of {0} failed. No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.", messageTypeName);
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

            return _interactionFacade.HandleNoResultsFoundInteraction(interaction);
        }

        private bool GetTotalnumberOfSearchResults(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress)
        {
            InteractionBase interaction = new GetTotalSearchResultsInteraction
            {
                WebDriver = webDriver,
                TotalNumberOfResults = searchUrlProgress.TotalSearchResults
            };

            return _interactionFacade.HandleGetTotalNumberOfSearchResults(interaction);
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
    }
}
