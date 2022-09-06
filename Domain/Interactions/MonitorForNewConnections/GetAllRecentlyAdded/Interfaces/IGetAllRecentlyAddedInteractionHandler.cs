using System.Collections.Generic;
using Domain.Models.MonitorForNewProspects;

namespace Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces
{
    public interface IGetAllRecentlyAddedInteractionHandler : IInteractionHandler
    {
        public IList<RecentlyAddedProspectModel> GetRecentlyAddedProspects();
    }
}
