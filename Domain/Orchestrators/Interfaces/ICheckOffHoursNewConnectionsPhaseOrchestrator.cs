using Domain.Models.MonitorForNewProspects;
using Domain.Models.RabbitMQMessages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface ICheckOffHoursNewConnectionsPhaseOrchestrator
    {
        public IList<RecentlyAddedProspect> RecentlyAddedProspects { get; }
        void Execute(CheckOffHoursNewConnectionsBody message);
    }
}
