using System.Collections.Generic;

namespace Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces
{
    public interface IGetAllRecentlyAddedInteractionHandler : IInteractionHandler
    {
        public IList<Models.RecentlyAddedProspect> GetRecentlyAddedProspects();
    }
}
