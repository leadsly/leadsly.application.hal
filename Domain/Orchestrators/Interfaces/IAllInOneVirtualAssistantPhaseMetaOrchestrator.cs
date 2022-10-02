using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.MQ.Messages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IAllInOneVirtualAssistantPhaseMetaOrchestrator
    {
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        public event NewMessagesReceivedEventHandler NewMessagesReceived;
        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects;
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent;
        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects;
        public event ConnectionsSentEventHandler ConnectionsSent;
        public event MonthlySearchLimitReachedEventHandler MonthlySearchLimitReached;
        public event UpdatedSearchUrlProgressEventHandler UpdatedSearchUrlsProgress;
        public void Execute(AllInOneVirtualAssistantMessageBody message);
    }
}
