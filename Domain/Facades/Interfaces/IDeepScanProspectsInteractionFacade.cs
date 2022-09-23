using Domain.Interactions;
using Domain.Models.DeepScanProspectsForReplies;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IDeepScanProspectsInteractionFacade
    {
        public ProspectRepliedModel ProspectReplied { get; }
        public IList<IWebElement> ProspectMessageListItems { get; }
        public int ConversationCount { get; }
        bool HandleClearMessagingCriteriaInteraction(InteractionBase interaction);
        bool HandleEnterSearchmessageCriteriaInteraction(InteractionBase interaction);
        bool HandleGetProspectsMessageItemInteraction(InteractionBase interaction);
        bool HandleCheckMessagesHistoryInteraction(InteractionBase interaction);
        bool HandleGetVisibleConversationsCountInteraction(InteractionBase interaction);
    }
}
