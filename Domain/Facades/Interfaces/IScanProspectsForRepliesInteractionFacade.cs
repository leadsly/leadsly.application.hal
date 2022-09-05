using Domain.Interactions;
using Domain.Models.ScanProspectsForReplies;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IScanProspectsForRepliesInteractionFacade
    {
        IList<IWebElement> NewMessageElements { get; }
        NewMessage NewMessage { get; }
        bool HandleGetNewMessagesInteraction(InteractionBase interaction);
        bool HandleGetMessageContentInteraction(InteractionBase interaction);
        bool HandleCloseConversationsInteraction(InteractionBase interaction);
    }
}
