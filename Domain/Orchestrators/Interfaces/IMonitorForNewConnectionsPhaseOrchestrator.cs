﻿using Domain.Executors.MonitorForNewConnections.Events;
using Domain.MQ.Messages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IMonitorForNewConnectionsPhaseOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        void Execute(MonitorForNewAcceptedConnectionsBody message);
    }
}
