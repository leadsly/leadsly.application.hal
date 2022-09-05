using System.Collections.Generic;
using Domain.Models.MonitorForNewProspects;

namespace Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces
{
    public interface IGetAllRecentlyAddedSinceInteractionHandler : IInteractionHandler
    {
        public IList<RecentlyAddedProspect> GetRecentlyAddedProspects();
    }
}
