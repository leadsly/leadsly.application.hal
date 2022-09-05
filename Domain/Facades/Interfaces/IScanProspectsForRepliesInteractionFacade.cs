using Domain.Interactions;
using Leadsly.Application.Model.Requests;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IScanProspectsForRepliesInteractionFacade
    {
        IList<IWebElement> NewMessages { get; }
        NewMessageRequest NewMessageRequest { get; }
        bool HandleGetNewMessagesInteraction(InteractionBase interaction);
        bool HandleGetMessageContentInteraction(InteractionBase interaction);
        bool HandleCloseConversationsInteraction(InteractionBase interaction);
    }
}
