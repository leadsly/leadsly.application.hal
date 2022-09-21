using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Models.AllInOneVirtualAssistant;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IAllInOneVirtualAssistantPhaseOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<ConnectionSentModel> ConnectionsSent { get; }
        public bool GetMonthlySearchLimitReached();
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrlsProgress();
        public IList<SentFollowUpMessageModel> GetSentFollowUpMessages();
        public PreviouslyConnectedNetworkProspectsModel GetPreviouslyConnectedNetworkProspects();
        void Execute(AllInOneVirtualAssistantMessageBody message);
    }
}
