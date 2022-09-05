using Domain.Executors.MonitorForNewConnections.Events;
using Leadsly.Application.Model.Campaigns;

namespace Domain.Orchestrators.Interfaces
{
    public interface IMonitorForNewConnectionsPhaseOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        void Execute(MonitorForNewAcceptedConnectionsBody message);
    }
}
