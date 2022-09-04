using Domain.Interactions;
using Domain.Models;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IDeepScanProspectsInteractionFacade
    {
        public ProspectReplied ProspectReplied { get; }
        public IList<IWebElement> ProspectMessageListItems { get; }
        bool HandleClearMessagingCriteriaInteraction(InteractionBase interaction);
        bool HandleEnterSearchmessageCriteriaInteraction(InteractionBase interaction);
        bool HandleGetProspectsMessageItemInteraction(InteractionBase interaction);
        bool HandleCheckMessagesHistoryInteraction(InteractionBase interaction);
    }
}
