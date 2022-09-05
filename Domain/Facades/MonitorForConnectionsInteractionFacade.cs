using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount.Interfaces;
using Domain.Interactions.Shared.CloseAllConversations.Interfaces;
using Domain.Interactions.Shared.RefreshBrowser.Interfaces;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class MonitorForConnectionsInteractionFacade : IMonitorForConnectionsInteractionFacade
    {
        public MonitorForConnectionsInteractionFacade(
            ICloseAllConversationsInteractionHandler closeAllConversationHandler,
            IGetAllRecentlyAddedInteractionHandler getAllRecentlyAddedHandler,
            IRefreshBrowserInteractionHandler refreshBrowserHandler,
            IGetConnectionsCountInteractionHandler getConnectionsCountHandler)
        {
            _refreshBrowserHandler = refreshBrowserHandler;
            _closeAllConversationsHandler = closeAllConversationHandler;
            _getAllRecentlyAddedHandler = getAllRecentlyAddedHandler;
            _getConnectionsCountHandler = getConnectionsCountHandler;
        }

        private readonly IRefreshBrowserInteractionHandler _refreshBrowserHandler;
        private readonly ICloseAllConversationsInteractionHandler _closeAllConversationsHandler;
        private readonly IGetAllRecentlyAddedInteractionHandler _getAllRecentlyAddedHandler;
        private readonly IGetConnectionsCountInteractionHandler _getConnectionsCountHandler;

        public int ConnectionsCount => _getConnectionsCountHandler.GetConnectionsCount();

        public IList<Models.RecentlyAddedProspect> RecentlyAddedProspects => _getAllRecentlyAddedHandler.GetRecentlyAddedProspects();

        public bool HandleCloseAllConversationsInteraction(InteractionBase interaction)
        {
            return _closeAllConversationsHandler.HandleInteraction(interaction);
        }

        public bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction)
        {
            return _getAllRecentlyAddedHandler.HandleInteraction(interaction);
        }

        public bool HandleGetConnectionsCountInteraction(InteractionBase interaction)
        {
            return _getConnectionsCountHandler.HandleInteraction(interaction);
        }

        public bool HandleRefreshBrowserInteraction(InteractionBase interaction)
        {
            return _refreshBrowserHandler.HandleInteraction(interaction);
        }
    }
}
