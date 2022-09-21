using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IMessageListBubbleServicePOM
    {
        IList<IWebElement> GetMessagesListBubbles(IWebDriver webDriver);
        IList<IWebElement> GetUnreadMessagesListBubbles(IList<IWebElement> messageListBubbles);
        bool ClickNewMessage(IWebElement newMessageListItem, IWebDriver webDriver);
        public string GetMessageContent(IWebElement conversationPopUp);
        string ProspectNameFromMessage(IWebElement element);
    }
}
