using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.ScanProspectsForReplies.GetNewMessages.Interfaces
{
    public interface IGetNewMessagesInteractionHandler : IInteractionHandler
    {
        public IList<IWebElement> GetNewMessages();
    }
}
