using Domain.Facades.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces;
using Domain.Models;

namespace Domain.Facades
{
    public class DeepScanProspectsInteractionFacade : IDeepScanProspectsInteractionFacade
    {
        public DeepScanProspectsInteractionFacade(
            IClearMessagingSearchCriteriaInteractionHandler<ClearMessagingSearchCrtieriaInteraction> clearMessagingSearchCriteriaHandler,
            IEnterSearchMessageCriteriaInteractionHandler<EnterSearchMessageCriteriaInteraction> enterSearchMessageCriteriaHandler,
            ICheckMessagesHistoryInteractionHandler<CheckMessagesHistoryInteraction> checkMessagesHistoryInteractionHandler,
            IGetProspectsMessageItemInteractionHandler<GetProspectsMessageItemInteraction> getProspectsMesageItemHandler)
        {
            _clearMessagingSearchCriteriaHandler = clearMessagingSearchCriteriaHandler;
            _enterSearchMessageCriteriaHandler = enterSearchMessageCriteriaHandler;
            _getProspectsMesageItemHandler = getProspectsMesageItemHandler;
            _checkMessagesHistoryInteractionHandler = checkMessagesHistoryInteractionHandler;
        }

        private readonly ICheckMessagesHistoryInteractionHandler<CheckMessagesHistoryInteraction> _checkMessagesHistoryInteractionHandler;
        private readonly IClearMessagingSearchCriteriaInteractionHandler<ClearMessagingSearchCrtieriaInteraction> _clearMessagingSearchCriteriaHandler;
        private readonly IEnterSearchMessageCriteriaInteractionHandler<EnterSearchMessageCriteriaInteraction> _enterSearchMessageCriteriaHandler;
        private readonly IGetProspectsMessageItemInteractionHandler<GetProspectsMessageItemInteraction> _getProspectsMesageItemHandler;

        public ProspectReplied ProspectReplied { get; set; }

        public bool HandleInteraction(ClearMessagingSearchCrtieriaInteraction interaction)
        {
            return _clearMessagingSearchCriteriaHandler.HandleInteraction(interaction);
        }

        public bool HandleInteraction(EnterSearchMessageCriteriaInteraction interaction)
        {
            return _enterSearchMessageCriteriaHandler.HandleInteraction(interaction);
        }

        public bool HandleInteraction(GetProspectsMessageItemInteraction interaction)
        {
            return _getProspectsMesageItemHandler.HandleInteraction(interaction);
        }

        public bool HandleInteraction(CheckMessagesHistoryInteraction interaction)
        {
            ProspectReplied = _checkMessagesHistoryInteractionHandler.Prospect;
            return _checkMessagesHistoryInteractionHandler.HandleInteraction(interaction);
        }
    }
}
