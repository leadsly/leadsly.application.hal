using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.InstructionSets.Interfaces;
using Domain.Models.Networking;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class NetworkingPhaseOrchestrator : PhaseOrchestratorBase, INetworkingPhaseOrchestrator
    {
        public NetworkingPhaseOrchestrator(
            ILogger<NetworkingPhaseOrchestrator> logger,
            IWebDriverProvider webDriverProvider,
            INetworkingInstructionSet instructionSet
            ) : base(logger)
        {

            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _instructionSet = instructionSet;
        }

        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects
        {
            add => _instructionSet.PersistPrimaryProspects += value;
            remove => _instructionSet.PersistPrimaryProspects -= value;
        }

        public event ConnectionsSentEventHandler ConnectionsSent;

        public event MonthlySearchLimitReachedEventHandler SearchLimitReached;

        public event UpdatedSearchUrlProgressEventHandler UpdatedSearchUrlsProgress;

        private readonly INetworkingInstructionSet _instructionSet;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<NetworkingPhaseOrchestrator> _logger;
        private bool MonthlySearchLimitReached => _instructionSet.MonthlySearchLimitReached;
        private int NumberOfConnectionsSent
        {
            get => _instructionSet.NumberOfConnectionsSent;
            set => _instructionSet.NumberOfConnectionsSent = value;
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

        public void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            if (message.NetworkingMessages != null)
            {
                Queue<NetworkingMessageBody> networkingMessages = message.NetworkingMessages;
                int length = networkingMessages.Count;
                for (int i = 0; i < length; i++)
                {
                    NetworkingMessageBody networkingMessage = networkingMessages.Dequeue();

                    ExecuteInternal(networkingMessage, webDriver, networkingMessage.SearchUrlsProgress);
                }
            }
        }

        private void ExecuteInternal(NetworkingMessageBody message, IWebDriver webDriver, IList<SearchUrlProgressModel> searchUrlsProgress)
        {
            try
            {
                BeginNetworking(message, webDriver, searchUrlsProgress);
            }
            finally
            {
                _webDriverProvider.CloseCurrentTab(webDriver, BrowserPurpose.Networking);
                NumberOfConnectionsSent = 0;

                // output the results here                
                OutputUpdateMonthlySearchLimit(message);
                OutputConnectionsSent(message);
                OutputUpdateSearchUrlsProgress(message);
            }
        }

        private void OutputUpdateMonthlySearchLimit(NetworkingMessageBody message)
        {
            IList<SearchUrlProgressModel> updatedSearchUrlsProgress = _instructionSet.GetUpdatedSearchUrls();
            if (updatedSearchUrlsProgress != null && updatedSearchUrlsProgress.Count > 0)
            {
                UpdatedSearchUrlsProgress.Invoke(this, new UpdatedSearchUrlProgressEventArgs(message, updatedSearchUrlsProgress));
            }
        }

        private void OutputConnectionsSent(NetworkingMessageBody message)
        {
            IList<ConnectionSentModel> connectionsSent = _instructionSet.GetConnectionsSent();
            if (connectionsSent != null && connectionsSent.Count > 0)
            {
                _logger.LogInformation("Connections Sent: {0}", connectionsSent.Count);
                ConnectionsSent.Invoke(this, new ConnectionsSentEventArgs(message, connectionsSent));
            }
        }

        private void OutputUpdateSearchUrlsProgress(NetworkingMessageBody message)
        {
            bool searchLimitReached = _instructionSet.GetMonthlySearchLimitReached();
            SearchLimitReached.Invoke(this, new MonthlySearchLimitReachedEventArgs(message, searchLimitReached));
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

                if (_instructionSet.GetTotalnumberOfSearchResultsInteraction(webDriver, searchUrlProgress) == false)
                {
                    // just move onto the next search url
                    _logger.LogDebug($"Unable to determine total number of search results. Moving on to the next search url in the list. This failure occured for search result: {searchUrlProgress.SearchUrl}");
                    continue;
                }

                int totalResults = _instructionSet.TotalNumberOfSearchResults;
                _instructionSet.Add_UpdateSearchUrlProgressRequest(searchUrlProgress.SearchUrlProgressId, searchUrlProgress.LastPage, webDriver.Url, totalResults, webDriver.CurrentWindowHandle);

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

            if (_instructionSet.NoSearchResultsDisplayedInteraction(webDriver) == true)
            {
                _logger.LogError("Execution of {0} failed. No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.", messageTypeName);
                return false;
            }

            return true;
        }
    }
}
