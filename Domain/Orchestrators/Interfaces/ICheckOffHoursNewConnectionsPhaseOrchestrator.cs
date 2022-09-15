using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface ICheckOffHoursNewConnectionsPhaseOrchestrator
    {
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        void Execute(CheckOffHoursNewConnectionsBody message);
    }
}
