using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class DeepScanProspectsInteractionFacade : IDeepScanProspectsInteractionFacade
    {
        public DeepScanProspectsInteractionFacade(
            IClearMessagingSearchCriteriaInteractionHandler clearMessagingSearchCriteriaHandler,
            IEnterSearchMessageCriteriaInteractionHandler enterSearchMessageCriteriaHandler,
            ICheckMessagesHistoryInteractionHandler checkMessagesHistoryInteractionHandler,
            IGetAllVisibleConversationCountInteractionHandler getVisibleConversationCountHandler,
            IGetProspectsMessageItemInteractionHandler getProspectsMesageItemHandler)
        {
            _clearMessagingSearchCriteriaHandler = clearMessagingSearchCriteriaHandler;
            _enterSearchMessageCriteriaHandler = enterSearchMessageCriteriaHandler;
            _getProspectsMesageItemHandler = getProspectsMesageItemHandler;
            _checkMessagesHistoryInteractionHandler = checkMessagesHistoryInteractionHandler;
            _getVisibleConversationCountHandler = getVisibleConversationCountHandler;
        }

        private readonly ICheckMessagesHistoryInteractionHandler _checkMessagesHistoryInteractionHandler;
        private readonly IGetAllVisibleConversationCountInteractionHandler _getVisibleConversationCountHandler;
        private readonly IClearMessagingSearchCriteriaInteractionHandler _clearMessagingSearchCriteriaHandler;
        private readonly IEnterSearchMessageCriteriaInteractionHandler _enterSearchMessageCriteriaHandler;
        private readonly IGetProspectsMessageItemInteractionHandler _getProspectsMesageItemHandler;

        public ProspectRepliedModel ProspectReplied { get; private set; }

        public IList<IWebElement> ProspectMessageListItems { get; private set; }

        public int ConversationCount { get; private set; }

        public bool HandleClearMessagingCriteriaInteraction(InteractionBase interaction)
        {
            return _clearMessagingSearchCriteriaHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterSearchmessageCriteriaInteraction(InteractionBase interaction)
        {
            return _enterSearchMessageCriteriaHandler.HandleInteraction(interaction);
        }

        public bool HandleGetProspectsMessageItemInteraction(InteractionBase interaction)
        {
            bool succeeded = _getProspectsMesageItemHandler.HandleInteraction(interaction);
            ProspectMessageListItems = _getProspectsMesageItemHandler.GetProspectMessageItems();
            return succeeded;
        }

        public bool HandleCheckMessagesHistoryInteraction(InteractionBase interaction)
        {
            bool succeeded = _checkMessagesHistoryInteractionHandler.HandleInteraction(interaction);
            ProspectReplied = _checkMessagesHistoryInteractionHandler.GetProspect();
            return succeeded;
        }

        public bool HandleGetVisibleConversationsCountInteraction(InteractionBase interaction)
        {
            bool succeeded = _getVisibleConversationCountHandler.HandleInteraction(interaction);
            ConversationCount = _getVisibleConversationCountHandler.GetConversationCount();
            return succeeded;
        }
    }
}
