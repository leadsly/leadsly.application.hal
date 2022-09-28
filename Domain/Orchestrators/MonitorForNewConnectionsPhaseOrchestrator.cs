using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount;
using Domain.Interactions.Shared.CloseAllConversations;
using Domain.Interactions.Shared.RefreshBrowser;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Orchestrators
{
    public class MonitorForNewConnectionsPhaseOrchestrator : PhaseOrchestratorBase, IMonitorForNewConnectionsPhaseOrchestrator
    {
        public MonitorForNewConnectionsPhaseOrchestrator(
            ILogger<MonitorForNewConnectionsPhaseOrchestrator> logger,
            ITimestampService timestampService,
            IMonitorForConnectionsInteractionFacade interactionsFacade,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _logger = logger;
            _interactionsFacade = interactionsFacade;
            _timestampService = timestampService;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IMonitorForConnectionsInteractionFacade _interactionsFacade;
        private readonly ILogger<MonitorForNewConnectionsPhaseOrchestrator> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IWebDriverProvider _webDriverProvider;
        public static bool IsRunning { get; set; } = false;

        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects;

        public void Execute(MonitorForNewAcceptedConnectionsBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(MonitorForNewAcceptedConnectionsBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.MonitorForNewAcceptedConnections, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            if (GoToPage(webDriver, message.PageUrl) == false)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not navigate to the given PageUrl {1}. HalId {2}", messageTypeName, message.PageUrl, message.HalId);
                return;
            }

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            BeginMonitoring(webDriver, message);
        }

        private void BeginMonitoring(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            DateTimeOffset endOfWorkDayLocal = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, message.EndOfWorkday);
            while (_timestampService.GetNowLocalized(message.TimeZoneId) < endOfWorkDayLocal)
            {
                bool getConnectionsCountSucceeded = GetConnectionsCount(webDriver);

                if (getConnectionsCountSucceeded == false)
                {
                    _logger.LogDebug("Failed to determine total connections count.");
                }

                bool getAllRecentlyAddedSucceeded = GetAllRecentlyAddedInteraction(webDriver);

                if (getAllRecentlyAddedSucceeded == false)
                {
                    _logger.LogDebug("Failed to gather all recently added prospects");
                }

                if (CloseAllActiveConversation(webDriver) == false)
                {
                    _logger.LogDebug("Failed to close all active conversations. That is ok continuing on with the phase");
                }

                if (RefreshBrowser(webDriver) == false)
                {
                    _logger.LogDebug("Failed to refresh the browser. Continuing on with the phase");
                }

                if (getConnectionsCountSucceeded == true && getAllRecentlyAddedSucceeded == true)
                {
                    int previousConnectionsCount = _interactionsFacade.ConnectionsCount;
                    IList<RecentlyAddedProspectModel> previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                    if (GetAllRecentlyAddedInteraction(webDriver) == true && GetConnectionsCount(webDriver))
                    {
                        IList<RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;
                        IList<RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

                        // invoke an event here
                        int newTotalConnectionsCount = _interactionsFacade.ConnectionsCount;
                        OutputRecentlyAddedProspects(message, newRecentlyAddedProspects, newTotalConnectionsCount);
                    }
                }

            }
        }

        public void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            ConnectedNetworkProspectsResponse previousMonitoredResponse = message.PreviousMonitoredResponse;
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
                    _logger.LogTrace("Executing {0}. Server has successfully set Items property on {1} response. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
                    int previousConnectionsCount = 0;
                    IList<RecentlyAddedProspectModel> previousRecentlyAddedProspects = default;

                    // set previous count and previous recently added prospects to whats on the page currently
                    if (previousMonitoredResponse.Items.Count == 0)
                    {
                        _logger.LogDebug("Executing {0}. {1} response did not contain any prospects to check against. Setting previous values from what is currently displayed on the page. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
                        // persist results to server
                        previousConnectionsCount = _interactionsFacade.ConnectionsCount;
                        previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                        OutputSaveRecentlyAddedProspects(message, previousConnectionsCount, previousRecentlyAddedProspects);
                    }
                    // or set it to whatever the server had saved before
                    else
                    {
                        _logger.LogDebug("Executing {0}. {1} response did contain prospects to check against. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
                        previousConnectionsCount = previousMonitoredResponse.TotalConnectionsCount;
                        previousRecentlyAddedProspects = previousMonitoredResponse.Items;
                    }

                    if (GetConnectionsCount(webDriver) == true)
                    {
                        int currentTotalConnectionsCount = _interactionsFacade.ConnectionsCount;
                        if (currentTotalConnectionsCount > previousConnectionsCount)
                        {
                            if (GetAllRecentlyAddedInteraction(webDriver) == true)
                            {
                                _logger.LogTrace("Executing {0}. Total connections count is greater than previous connection count.", nameof(AllInOneVirtualAssistantMessageBody));
                                IList<RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                                OutputSaveRecentlyAddedProspects(message, currentTotalConnectionsCount, currentRecentlyAddedProspects);

                                IList<RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

                                // invoke an event here                                
                                OutputRecentlyAddedProspects(message, newRecentlyAddedProspects, currentTotalConnectionsCount);
                            }
                        }
                    }
                }
            }
        }

        private void OutputRecentlyAddedProspects(MonitorForNewAcceptedConnectionsBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects, int currentTotalConnectionsCount)
        {
            if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
            {
                this.NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects, currentTotalConnectionsCount));
            }
        }

        private bool CloseAllActiveConversation(IWebDriver webDriver)
        {
            InteractionBase interaction = new CloseAllConversationsInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleCloseAllConversationsInteraction(interaction);
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

        private bool RefreshBrowser(IWebDriver webDriver)
        {
            InteractionBase interaction = new RefreshBrowserInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleRefreshBrowserInteraction(interaction);
        }

        private void OutputRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects, int newTotalConnectionsCount)
        {
            if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
            {
                _logger.LogDebug("Executing {0}. Emitting new connections detected event", nameof(AllInOneVirtualAssistantMessageBody));
                NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects, newTotalConnectionsCount));
            }
        }

        private void OutputSaveRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, int totalConnectionsCount, IList<RecentlyAddedProspectModel> recentlyAddedProspects)
        {
            _logger.LogDebug("Executing {0}. Saving recently added prospects", nameof(AllInOneVirtualAssistantMessageBody));
            UpdateRecentlyAddedProspects.Invoke(this, new UpdateRecentlyAddedProspectsEventArgs(message, recentlyAddedProspects, totalConnectionsCount));
        }
    }
}
