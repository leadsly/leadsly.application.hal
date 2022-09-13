using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount;
using Domain.Interactions.Shared.CloseAllConversations;
using Domain.Interactions.Shared.RefreshBrowser;
using Domain.Models.RabbitMQMessages;
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

        public void Execute(MonitorForNewAcceptedConnectionsBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(MonitorForNewAcceptedConnectionsBody);
            _logger.LogInformation("Executing DeepScanProspectsForRepliesBody on hal id {halId}", halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.MonitorForNewAcceptedConnections, message.ChromeProfileName, message.GridNamespaceName, message.GridServiceDiscoveryName, out bool isNewWebDriver);
            if (webDriver == null)
            {
                _logger.LogError("WebDriver could not be found or created. Cannot proceed. HalId {0} executing {1}", halId, messageTypeName);
                return;
            }

            if (GoToPage(webDriver, message.PageUrl) == false)
            {
                _logger.LogError("WebDriver could not navigate to {0} when executing {1}. HalId {2}", message.PageUrl, messageTypeName, halId);
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
                    IList<Models.MonitorForNewProspects.RecentlyAddedProspectModel> previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                    if (GetAllRecentlyAddedInteraction(webDriver) == true)
                    {
                        IList<Models.MonitorForNewProspects.RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;
                        IList<Models.MonitorForNewProspects.RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

                        // invoke an event here
                        OutputRecentlyAddedProspects(message, newRecentlyAddedProspects);
                    }
                }

            }
        }

        private void OutputRecentlyAddedProspects(MonitorForNewAcceptedConnectionsBody message, IList<Models.MonitorForNewProspects.RecentlyAddedProspectModel> newRecentlyAddedProspects)
        {
            if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
            {
                this.NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects));
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
    }
}
