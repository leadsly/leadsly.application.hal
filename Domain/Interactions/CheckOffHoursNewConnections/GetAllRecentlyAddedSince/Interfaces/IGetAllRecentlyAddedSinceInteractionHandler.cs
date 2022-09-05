using System.Collections.Generic;

namespace Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces
{
    public interface IGetAllRecentlyAddedSinceInteractionHandler : IInteractionHandler
    {
        public IList<Models.RecentlyAddedProspect> GetRecentlyAddedProspects();
    }
}
