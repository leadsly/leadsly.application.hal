using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.MQ.Messages;
using OpenQA.Selenium;

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

        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects;
        public event ConnectionsSentEventHandler ConnectionsSent;
        public event MonthlySearchLimitReachedEventHandler MonthlySearchLimitReached;
        public event UpdatedSearchUrlProgressEventHandler UpdatedSearchUrlsProgress;
        public void HandleCheckOffHoursNewConnections(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message);
        public void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleFollowUpMessages(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
        public void HandleNetworking(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
    }
}
