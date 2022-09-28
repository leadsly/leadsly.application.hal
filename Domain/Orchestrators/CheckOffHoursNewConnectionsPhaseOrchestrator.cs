using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.InstructionSets.Interfaces;
using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
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
            ICheckForNewConnectionsFromOffHoursInstructionSet instructionSet,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _logger = logger;
            _instructionSet = instructionSet;
            _webDriverProvider = webDriverProvider;
        }

        private readonly ICheckForNewConnectionsFromOffHoursInstructionSet _instructionSet;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<CheckOffHoursNewConnectionsPhaseOrchestrator> _logger;

        public event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected;
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects => _instructionSet.RecentlyAddedProspects;

        public void Execute(CheckOffHoursNewConnectionsBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(CheckOffHoursNewConnectionsBody);
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

        public void Execute(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            try
            {
                ExecuteInternal(webDriver, message);
            }
            finally
            {
                OutputCheckOffHoursNewConnections(message);
            }
        }

        private void ExecuteInternal(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            _instructionSet.BeginCheckingForNewConnectionsFromOffHours(webDriver, message);
        }

        private void OutputCheckOffHoursNewConnections(CheckOffHoursNewConnectionsBody message)
        {
            IList<RecentlyAddedProspectModel> recentlyAddedProspects = _instructionSet.RecentlyAddedProspects;
            if (recentlyAddedProspects != null && recentlyAddedProspects.Count > 0)
            {
                _logger.LogDebug("{0} found new connections!", nameof(CheckOffHoursNewConnectionsBody));
                // -1 means it is checkoffhours so do not update total new connections property
                this.OffHoursNewConnectionsDetected.Invoke(this, new OffHoursNewConnectionsEventArgs(message, recentlyAddedProspects));
            }
        }

        //private void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        //{
        //    bool succeeded = GetRecentlyAdded(webDriver, message);
        //    if (succeeded == false)
        //    {
        //        _logger.LogDebug("Failed to get recently added prospects from CheckOffHoursConnectionsPhase");
        //    }
        //}

        //private bool GetRecentlyAdded(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        //{
        //    InteractionBase interaction = new GetAllRecentlyAddedSinceInteraction
        //    {
        //        WebDriver = webDriver,
        //        NumOfHoursAgo = message.NumOfHoursAgo,
        //        TimezoneId = message.TimeZoneId
        //    };

        //    return _interactionHandler.HandleInteraction(interaction);
        //}
    }
}
