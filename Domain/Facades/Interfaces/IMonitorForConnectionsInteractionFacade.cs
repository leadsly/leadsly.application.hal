using Domain.Interactions;
using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IMonitorForConnectionsInteractionFacade
    {
        public int ConnectionsCount { get; }
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        bool HandleGetConnectionsCountInteraction(InteractionBase interaction);
        bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction);
        bool HandleRefreshBrowserInteraction(InteractionBase interaction);
        bool HandleCloseAllConversationsInteraction(InteractionBase interaction);
    }
}
