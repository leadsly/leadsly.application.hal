using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Facades.Interfaces;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace Domain.Orchestrators
{
    public class AllInOneVirtualAssistantPhaseMetaOrchestrator : PhaseOrchestratorBase, IAllInOneVirtualAssistantPhaseMetaOrchestrator
    {
        private const string PRIMARY_PAGE_URL = "https://www.linkedin.com/mynetwork/invite-connect/connections/";

        public AllInOneVirtualAssistantPhaseMetaOrchestrator(
            ILogger<AllInOneVirtualAssistantPhaseMetaOrchestrator> logger,
            IAllInOneOrchestratorsFacade orchestratorsFacade,
            IWebDriverProvider webDriverProvider) : base(logger)
        {
            _orchestratorsFacade = orchestratorsFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IAllInOneOrchestratorsFacade _orchestratorsFacade;
        private readonly ILogger<AllInOneVirtualAssistantPhaseMetaOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;

        #region Events

        public event NewMessagesReceivedEventHandler NewMessagesReceived
        {
            add => _orchestratorsFacade.NewMessagesReceived += value;
            remove => _orchestratorsFacade.NewMessagesReceived -= value;
        }
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected
        {
            add => _orchestratorsFacade.NewConnectionsDetected += value;
            remove => _orchestratorsFacade.NewConnectionsDetected -= value;
        }

        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects
        {
            add => _orchestratorsFacade.UpdateRecentlyAddedProspects += value;
            remove => _orchestratorsFacade.UpdateRecentlyAddedProspects -= value;
        }

        public event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected
        {
            add => _orchestratorsFacade.OffHoursNewConnectionsDetected += value;
            remove => _orchestratorsFacade.OffHoursNewConnectionsDetected -= value;
        }

        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected
        {
            add => _orchestratorsFacade.ProspectsThatRepliedDetected += value;
            remove => _orchestratorsFacade.ProspectsThatRepliedDetected -= value;
        }

        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent
        {
            add => _orchestratorsFacade.FollowUpMessagesSent += value;
            remove => _orchestratorsFacade.FollowUpMessagesSent -= value;
        }

        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects
        {
            add => _orchestratorsFacade.PersistPrimaryProspects += value;
            remove => _orchestratorsFacade.PersistPrimaryProspects -= value;
        }

        public event ConnectionsSentEventHandler ConnectionsSent
        {
            add => _orchestratorsFacade.ConnectionsSent += value;
            remove => _orchestratorsFacade.ConnectionsSent -= value;
        }

        public event MonthlySearchLimitReachedEventHandler MonthlySearchLimitReached
        {
            add => _orchestratorsFacade.MonthlySearchLimitReached += value;
            remove => _orchestratorsFacade.MonthlySearchLimitReached -= value;
        }

        public event UpdatedSearchUrlProgressEventHandler UpdatedSearchUrlsProgress
        {
            add => _orchestratorsFacade.UpdatedSearchUrlsProgress += value;
            remove => _orchestratorsFacade.UpdatedSearchUrlsProgress -= value;
        }

        #endregion

        public void Execute(AllInOneVirtualAssistantMessageBody message)
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

            if (GoToPage(webDriver, PRIMARY_PAGE_URL) == false)
            {
                _logger.LogError("Failed to navigate to {0}. This phase will exist and nothing else will be executed.", PRIMARY_PAGE_URL);
                return;
            }

            PrimaryWindowHandle = webDriver.CurrentWindowHandle;

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            try
            {
                BeginVirtualAssistantWork(webDriver, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(AllInOneVirtualAssistantMessageBody));
            }
        }

        private void BeginVirtualAssistantWork(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            //if (message.DeepScanProspectsForReplies != null)
            //{
            // 1. deep scan prospects for replies
            // _orchestratorsFacade.HandleDeepScanProspectsForReplies(webDriver, message.DeepScanProspectsForReplies);

            //// before the follow up message is sent out lets make sure that deepscanprospectsfor replies did not find the prospect in our inbox and one that has replied already
            //IEnumerable<FollowUpMessageBody> followUpMessages = message.FollowUpMessages.Where(f => _orchestratorsFacade.ProspectsThatReplied.Any(x => x.Name == f.ProspectName) == false);
            //message.FollowUpMessages = new Queue<FollowUpMessageBody>(followUpMessages);
            // }

            if (message.CheckOffHoursNewConnections != null)
            {
                // 1. check off hours connections
                _orchestratorsFacade.HandleCheckOffHoursNewConnections(webDriver, message.CheckOffHoursNewConnections);
            }

            // 2. start with monitor for new connections
            _orchestratorsFacade.HandleMonitorForNewConnections(webDriver, message);

            // 3. then execute scan prospects for replies
            _orchestratorsFacade.HandleScanProspectsForReplies(webDriver, message);

            // 4. run follow up messages
            _orchestratorsFacade.HandleFollowUpMessages(webDriver, message);

            // 5. run networking 
            _orchestratorsFacade.HandleNetworking(webDriver, message);
        }
    }
}
