using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Facades.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class AllInOneOrchestratorsFacade : IAllInOneOrchestratorsFacade
    {
        public AllInOneOrchestratorsFacade(
            ICheckOffHoursNewConnectionsPhaseOrchestrator offHoursOrchestrator,
            IDeepScanProspectsForRepliesPhaseOrchestrator deepScanOrchestrator,
            IFollowUpMessagePhaseOrchestrator followUpOrchestrator,
            IMonitorForNewConnectionsPhaseOrchestrator monitorForNewConnectionsOrchestrator,
            INetworkingPhaseOrchestrator networkingOrchestrator,
            IScanProspectsForRepliesPhaseOrchestrator scanProspectsForRepliesOrchestrator)
        {
            _offHOursOrchestrator = offHoursOrchestrator;
            _deepScanOrchestrator = deepScanOrchestrator;
            _followUpOrchestrator = followUpOrchestrator;
            _monitorForNewConnectionsOrchestrator = monitorForNewConnectionsOrchestrator;
            _networkingOrchestrator = networkingOrchestrator;
            _scanProspectsForRepliesOrchestrator = scanProspectsForRepliesOrchestrator;
        }

        private readonly ICheckOffHoursNewConnectionsPhaseOrchestrator _offHOursOrchestrator;
        private readonly IDeepScanProspectsForRepliesPhaseOrchestrator _deepScanOrchestrator;
        private readonly IFollowUpMessagePhaseOrchestrator _followUpOrchestrator;
        private readonly IMonitorForNewConnectionsPhaseOrchestrator _monitorForNewConnectionsOrchestrator;
        private readonly INetworkingPhaseOrchestrator _networkingOrchestrator;
        private readonly IScanProspectsForRepliesPhaseOrchestrator _scanProspectsForRepliesOrchestrator;

        public IList<ProspectRepliedModel> ProspectsThatReplied => _deepScanOrchestrator.Prospects;
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects => _networkingOrchestrator.PersistPrimaryProspects;
        public bool MonthlySearchLimitReached => _networkingOrchestrator.GetMonthlySearchLimitReached();
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress => _networkingOrchestrator.UpdatedSearchUrlsProgress;
        public IList<SentFollowUpMessageModel> SentFollowUpMessages => _followUpOrchestrator.GetSentFollowUpMessages();
        public IList<ConnectionSentModel> ConnectionsSent => _networkingOrchestrator.ConnectionsSent;

        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent
        {
            add => _followUpOrchestrator.FollowUpMessagesSent += value;
            remove => _followUpOrchestrator.FollowUpMessagesSent -= value;
        }

        public event NewMessagesReceivedEventHandler NewMessagesReceived
        {
            add => _scanProspectsForRepliesOrchestrator.NewMessagesReceived += value;
            remove => _scanProspectsForRepliesOrchestrator.NewMessagesReceived -= value;
        }

        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected
        {
            add => _monitorForNewConnectionsOrchestrator.NewConnectionsDetected += value;
            remove => _monitorForNewConnectionsOrchestrator.NewConnectionsDetected -= value;
        }

        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects
        {
            add => _monitorForNewConnectionsOrchestrator.UpdateRecentlyAddedProspects += value;
            remove => _monitorForNewConnectionsOrchestrator.UpdateRecentlyAddedProspects -= value;
        }

        public event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected
        {
            add => _offHOursOrchestrator.OffHoursNewConnectionsDetected += value;
            remove => _offHOursOrchestrator.OffHoursNewConnectionsDetected -= value;
        }

        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected
        {
            add => _deepScanOrchestrator.ProspectsThatRepliedDetected += value;
            remove => _deepScanOrchestrator.ProspectsThatRepliedDetected -= value;
        }

        public void HandleCheckOffHoursNewConnections(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            _offHOursOrchestrator.Execute(webDriver, message);
        }

        public void HandleDeepScanProspectsForReplies(IWebDriver webDriver, DeepScanProspectsForRepliesBody message)
        {
            _deepScanOrchestrator.Execute(webDriver, message);
        }

        public void HandleFollowUpMessages(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            _followUpOrchestrator.Execute(webDriver, message);
        }

        public void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            _monitorForNewConnectionsOrchestrator.Execute(webDriver, message);
        }

        public void HandleNetworking(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            _networkingOrchestrator.Execute(webDriver, message);
        }

        public void HandleScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            _scanProspectsForRepliesOrchestrator.Execute(webDriver, message);
        }
    }
}
