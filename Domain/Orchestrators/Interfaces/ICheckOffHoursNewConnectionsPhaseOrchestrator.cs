using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface ICheckOffHoursNewConnectionsPhaseOrchestrator
    {
        public IList<Models.RecentlyAddedProspect> RecentlyAddedProspects { get; }
        void Execute(CheckOffHoursNewConnectionsBody message);
    }
}
