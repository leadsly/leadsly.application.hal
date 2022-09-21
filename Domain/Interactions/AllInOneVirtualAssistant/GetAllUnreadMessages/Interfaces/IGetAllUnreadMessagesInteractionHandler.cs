using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages.Interfaces
{
    public interface IGetAllUnreadMessagesInteractionHandler : IInteractionHandler
    {
        public IList<IWebElement> GetUnreadMessages();
    }
}
