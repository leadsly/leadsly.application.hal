using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent.Interfaces;
using Domain.Interactions.FollowUpMessage.CreateNewMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterProspectName.Interfaces;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount.Interfaces;
using Domain.Interactions.Networking.GetTotalSearchResults.Interfaces;
using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.ScanProspectsForReplies;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class AllInOneVirtualAssistantInteractionFacade : IAllInOneVirtualAssistantInteractionFacade
    {
        public AllInOneVirtualAssistantInteractionFacade(
            IGetAllRecentlyAddedInteractionHandler getAllRecentlyAddedHandler,
            IGetConnectionsCountInteractionHandler getConnectionsCountHandler,
            IGetAllUnreadMessagesInteractionHandler getAllUnreadMessagesHandler,
            IGetMessagesContentInteractionHandler getUnreadMessagesContentHandler,
            ICreateNewMessageInteractionHandler createNewMessageHandler,
            IEnterMessageInteractionHandler enterMessageHandler,
            IEnterProspectNameInteractionHandler enterProspectNameHandler,
            INoResultsFoundInteractionHandler noSearchResultsHandler,
            IGetTotalSearchResultsInteractionHandler getTotalSearchResultsHandler
            )
        {
            _getUnreadMessagesContentHandler = getUnreadMessagesContentHandler;
            _getAllUnreadMessagesHandler = getAllUnreadMessagesHandler;
            _getAllRecentlyAddedHandler = getAllRecentlyAddedHandler;
            _getConnectionsCountHandler = getConnectionsCountHandler;
            _createNewMessageHandler = createNewMessageHandler;
            _enterMessageHandler = enterMessageHandler;
            _enterProspectNameHandler = enterProspectNameHandler;
            _noSearchResultsHandler = noSearchResultsHandler;
            _getTotalSearchResultsHandler = getTotalSearchResultsHandler;
        }

        private readonly IGetMessagesContentInteractionHandler _getUnreadMessagesContentHandler;
        private readonly IGetAllRecentlyAddedInteractionHandler _getAllRecentlyAddedHandler;
        private readonly IGetConnectionsCountInteractionHandler _getConnectionsCountHandler;
        private readonly IGetAllUnreadMessagesInteractionHandler _getAllUnreadMessagesHandler;
        private readonly ICreateNewMessageInteractionHandler _createNewMessageHandler;
        private readonly IEnterMessageInteractionHandler _enterMessageHandler;
        private readonly IEnterProspectNameInteractionHandler _enterProspectNameHandler;
        private readonly INoResultsFoundInteractionHandler _noSearchResultsHandler;
        private readonly IGetTotalSearchResultsInteractionHandler _getTotalSearchResultsHandler;

        public int ConnectionsCount => _getConnectionsCountHandler.GetConnectionsCount();

        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects => _getAllRecentlyAddedHandler.GetRecentlyAddedProspects();

        public IList<IWebElement> UnreadMessages => _getAllUnreadMessagesHandler.GetUnreadMessages();

        public IList<NewMessageModel> NewMessages => _getUnreadMessagesContentHandler.GetNewMessages();

        public SentFollowUpMessageModel SentFollowUpMessage => _enterMessageHandler.GetSentFollowUpMessageModel();

        public int TotalNumberOfSearchResults => _getTotalSearchResultsHandler.GetTotalResults();

        public bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction)
        {
            return _getAllRecentlyAddedHandler.HandleInteraction(interaction);
        }

        public bool HandleGetConnectionsCountInteraction(InteractionBase interaction)
        {
            return _getConnectionsCountHandler.HandleInteraction(interaction);
        }

        public bool HandleGetAllUnreadMessageListBubbles(InteractionBase interaction)
        {
            return _getAllUnreadMessagesHandler.HandleInteraction(interaction);
        }

        public bool HandleGetUnreadMessagesContent(InteractionBase interaction)
        {
            return _getUnreadMessagesContentHandler.HandleInteraction(interaction);
        }

        public bool HandleCreateNewMessageInteraction(InteractionBase interaction)
        {
            return _createNewMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterMessageInteraction(InteractionBase interaction)
        {
            return _enterMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterProspectNameInteraction(InteractionBase interaction)
        {
            return _enterProspectNameHandler.HandleInteraction(interaction);
        }
        public bool HandleNoResultsFoundInteraction(InteractionBase interaction)
        {
            return _noSearchResultsHandler.HandleInteraction(interaction);
        }

        public bool HandleGetTotalNumberOfSearchResults(InteractionBase interaction)
        {
            return _getTotalSearchResultsHandler.HandleInteraction(interaction);
        }
    }
}
