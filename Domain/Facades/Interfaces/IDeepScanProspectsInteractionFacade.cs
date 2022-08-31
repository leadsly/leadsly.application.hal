using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem;
using Domain.Models;

namespace Domain.Facades.Interfaces
{
    public interface IDeepScanProspectsInteractionFacade
    {
        public ProspectReplied ProspectReplied { get; }
        bool HandleInteraction(ClearMessagingSearchCrtieriaInteraction interaction);
        bool HandleInteraction(EnterSearchMessageCriteriaInteraction interaction);
        bool HandleInteraction(GetProspectsMessageItemInteraction interaction);
        bool HandleInteraction(CheckMessagesHistoryInteraction interaction);
    }
}
