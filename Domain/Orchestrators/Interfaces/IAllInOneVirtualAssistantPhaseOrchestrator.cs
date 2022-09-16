using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Models.AllInOneVirtualAssistant;
using Domain.Models.Responses;
using Domain.MQ.Messages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IAllInOneVirtualAssistantPhaseOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        public PreviouslyConnectedNetworkProspectsModel PreviouslyConnectedNetworkProspects { get; }
        void Execute(AllInOneVirtualAssistantMessageBody message, PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse, PreviouslyScannedForRepliesProspectsResponse previousScannedResponse);
    }
}
