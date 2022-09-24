using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IAllInOneVirtualAssistantPhaseMetaOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects;
        event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        event FollowUpMessagesSentEventHandler FollowUpMessagesSent;
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<ConnectionSentModel> ConnectionsSent { get; }
        public bool MonthlySearchLimitReached { get; }
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; }
        public IList<SentFollowUpMessageModel> SentFollowUpMessages { get; }
        void Execute(AllInOneVirtualAssistantMessageBody message);
    }
}
