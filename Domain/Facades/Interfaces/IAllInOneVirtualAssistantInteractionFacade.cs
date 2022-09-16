using Domain.Interactions;
using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IAllInOneVirtualAssistantInteractionFacade
    {
        public int ConnectionsCount { get; }
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        bool HandleGetConnectionsCountInteraction(InteractionBase interaction);
        bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction);
    }
}
