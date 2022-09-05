using Domain.Interactions;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.RabbitMQMessages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class CheckOffHoursNewConnectionsPhaseOrchestrator : PhaseOrchestratorBase, ICheckOffHoursNewConnectionsPhaseOrchestrator
    {
        public CheckOffHoursNewConnectionsPhaseOrchestrator(
            ILogger<CheckOffHoursNewConnectionsPhaseOrchestrator> logger,
            IGetAllRecentlyAddedSinceInteractionHandler interactionHandler,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _logger = logger;
            _interactionHandler = interactionHandler;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IGetAllRecentlyAddedSinceInteractionHandler _interactionHandler;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<CheckOffHoursNewConnectionsPhaseOrchestrator> _logger;

        public IList<RecentlyAddedProspect> RecentlyAddedProspects => _interactionHandler.GetRecentlyAddedProspects();

        public void Execute(CheckOffHoursNewConnectionsBody message)
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing DeepScanProspectsForRepliesBody on hal id {halId}", halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.ScanForReplies, message.ChromeProfileName, message.GridNamespaceName, message.GridServiceDiscoveryName, out bool isNewWebDriver);
            if (webDriver == null)
            {
                _logger.LogError("WebDriver could not be found or created. Cannot proceed");
                return;
            }

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            BeginCheckingForNewConnectionsFromOffHours(webDriver, message);
        }

        private void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            bool succeeded = GetRecentlyAdded(webDriver, message);
            if (succeeded == false)
            {
                _logger.LogDebug("Failed to get recently added prospects from CheckOffHoursConnectionsPhase");
            }
        }

        private bool GetRecentlyAdded(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            InteractionBase interaction = new GetAllRecentlyAddedSinceInteraction
            {
                WebDriver = webDriver,
                NumOfHoursAgo = message.NumOfHoursAgo,
                TimezoneId = message.TimezoneId
            };

            return _interactionHandler.HandleInteraction(interaction);
        }
    }
}
