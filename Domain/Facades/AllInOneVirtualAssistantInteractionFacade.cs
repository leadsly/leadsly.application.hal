using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class AllInOneVirtualAssistantInteractionFacade : IAllInOneVirtualAssistantInteractionFacade
    {
        public AllInOneVirtualAssistantInteractionFacade(
            IMonitorForConnectionsInteractionFacade monitorFacade
            )
        {
            _monitorFacade = monitorFacade;
        }

        private readonly IMonitorForConnectionsInteractionFacade _monitorFacade;

        public int ConnectionsCount => _monitorFacade.ConnectionsCount;

        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects => _monitorFacade.RecentlyAddedProspects;

        public bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction)
        {
            return _monitorFacade.HandleGetAllRecentlyAddedInteraction(interaction);
        }

        public bool HandleGetConnectionsCountInteraction(InteractionBase interaction)
        {
            return _monitorFacade.HandleGetConnectionsCountInteraction(interaction);
        }
    }
}
