using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount;
using Domain.Models.AllInOneVirtualAssistant;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Orchestrators
{
    public class AllInOneVirtualAssistantPhaseOrchestrator : PhaseOrchestratorBase, IAllInOneVirtualAssistantPhaseOrchestrator
    {
        public AllInOneVirtualAssistantPhaseOrchestrator(
            ILogger<AllInOneVirtualAssistantPhaseOrchestrator> logger,
            IAllInOneVirtualAssistantInteractionFacade interactionsFacade,
            IWebDriverProvider webDriverProvider) : base(logger)
        {
            _interactionsFacade = interactionsFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        public PreviouslyConnectedNetworkProspectsModel PreviouslyConnectedNetworkProspects { get; private set; } = new();
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        private readonly IAllInOneVirtualAssistantInteractionFacade _interactionsFacade;
        private readonly ILogger<AllInOneVirtualAssistantPhaseOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;

        public void Execute(AllInOneVirtualAssistantMessageBody message, PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse, PreviouslyScannedForRepliesProspectsResponse previousScannedResponse)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(AllInOneVirtualAssistantMessageBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.AllInOne, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            ExecuteInternal(webDriver, message, previousMonitoredResponse, previousScannedResponse);
        }

        private void ExecuteInternal(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message, PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse, PreviouslyScannedForRepliesProspectsResponse previousScannedResponse)
        {
            try
            {
                BeginVirtualAssistantWork(webDriver, message, previousMonitoredResponse, previousScannedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(AllInOneVirtualAssistantMessageBody));
            }
        }

        private void BeginVirtualAssistantWork(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message, PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse, PreviouslyScannedForRepliesProspectsResponse previousScannedResponse)
        {
            // 1. start with monitor for new connections
            HandleMonitorForNewConnections(webDriver, message, previousMonitoredResponse);

            // 2. then execute scan prospects for replies

            // 3. then check to see if there are any follow up messages that need to go out

        }

        #region MonitorForNewConnections

        private void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message, PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse)
        {
            bool getConnectionsCountSucceeded = GetConnectionsCount(webDriver);

            if (getConnectionsCountSucceeded == false)
            {
                _logger.LogDebug("Executing {0}. Failed to determine total connections count. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            bool getAllRecentlyAddedSucceeded = GetAllRecentlyAddedInteraction(webDriver);

            if (getAllRecentlyAddedSucceeded == false)
            {
                _logger.LogDebug("Executing {0}. Failed to gather all recently added prospects. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            if (getConnectionsCountSucceeded == true && getAllRecentlyAddedSucceeded == true)
            {
                // ensure response Items is not null
                if (previousMonitoredResponse.Items != null)
                {
                    _logger.LogTrace("Executing {0}. Server has successfully set Items property on {1} response. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                    int previousConnectionsCount = 0;
                    IList<RecentlyAddedProspectModel> previousRecentlyAddedProspects = default;

                    // set previous count and previous recently added prospects to whats on the page currently
                    if (previousMonitoredResponse.Items.Count == 0)
                    {
                        _logger.LogDebug("Executing {0}. {1} response did not contain any prospects to check against. Setting previous values from what is currently displayed on the page. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                        // persist results to server
                        previousConnectionsCount = _interactionsFacade.ConnectionsCount;
                        previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                        PreviouslyConnectedNetworkProspects.PreviousTotalConnectionsCount = previousConnectionsCount;
                        PreviouslyConnectedNetworkProspects.Items = previousRecentlyAddedProspects;
                    }
                    // or set it to whatever the server had saved before
                    else
                    {
                        _logger.LogDebug("Executing {0}. {1} response did contain prospects to check against. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                        previousConnectionsCount = previousMonitoredResponse.PreviousTotalConnectionsCount;
                        previousRecentlyAddedProspects = previousMonitoredResponse.Items;
                    }

                    if (GetAllRecentlyAddedInteraction(webDriver) == true)
                    {
                        int currentTotalConnectionsCount = _interactionsFacade.ConnectionsCount;
                        if (currentTotalConnectionsCount > previousConnectionsCount)
                        {
                            _logger.LogTrace("Executing {0}. Total connections count is greater than previous connection count.", nameof(AllInOneVirtualAssistantMessageBody));
                            IList<RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;
                            IList<RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

                            // invoke an event here
                            OutputRecentlyAddedProspects(message, newRecentlyAddedProspects);
                        }
                    }
                }
            }
        }

        private bool GetConnectionsCount(IWebDriver webDriver)
        {
            InteractionBase interaction = new GetConnectionsCountInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetConnectionsCountInteraction(interaction);
        }

        private bool GetAllRecentlyAddedInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new GetAllRecentlyAddedInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetAllRecentlyAddedInteraction(interaction);
        }

        private void OutputRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects)
        {
            if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
            {
                _logger.LogDebug("Executing {0}. Emitting new connections detected event", nameof(AllInOneVirtualAssistantMessageBody));
                this.NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects));
            }
        }

        #endregion

        #region ScanProspectsForReplies

        // 1. Check for any new message notifications (the blue number next to prospect)
        // if any exist process as expected

        // run the below steps only once
        // 2. set all messages list bubbles from either the server or current page

        // 3. grab all messages list bubbles from the current page

        // 4. check if any new prospects appear in the list

        // 5. if they do process as expected

        // 6. just scan the page for new notification icons and pop up messages

        // 7. on new pop up message, extract details and process as expected, then close the pop up

        // 8. on new notification icon, open the conversation and repeat step 7

        #endregion
    }
}
