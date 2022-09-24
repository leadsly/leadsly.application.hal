using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IAllInOneOrchestratorsFacade
    {
        public event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected;
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects;
        public event NewMessagesReceivedEventHandler NewMessagesReceived;
        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent;

        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<ConnectionSentModel> ConnectionsSent { get; }
        public bool MonthlySearchLimitReached { get; }
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; }
        public IList<SentFollowUpMessageModel> SentFollowUpMessages { get; }
        public void HandleDeepScanProspectsForReplies(IWebDriver webDriver, DeepScanProspectsForRepliesBody message);
        public void HandleCheckOffHoursNewConnections(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message);
        public void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleFollowUpMessages(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleNetworking(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
    }
}
