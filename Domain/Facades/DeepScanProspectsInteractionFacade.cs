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
using OpenQA.Selenium;
using System.Collections.Generic;

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

        public IList<IWebElement> ProspectMessageListItems { get; set; }

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
            bool succeeded = _getProspectsMesageItemHandler.HandleInteraction(interaction);
            ProspectMessageListItems = _getProspectsMesageItemHandler.GetProspectMessageItems();
            return succeeded;
        }

        public bool HandleInteraction(CheckMessagesHistoryInteraction interaction)
        {
            bool succeeded = _checkMessagesHistoryInteractionHandler.HandleInteraction(interaction);
            ProspectReplied = _checkMessagesHistoryInteractionHandler.GetProspect();
            return succeeded;
        }
    }
}
