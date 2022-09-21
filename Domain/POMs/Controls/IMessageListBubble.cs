using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Controls
{
    public interface IMessageListBubble
    {
        public bool MessageListBubblesControl(IWebDriver webDriver);
        public IList<IWebElement> GetAllMessagesListBubbles(IWebDriver webDriver);
        bool UnreadMessageNotificationExists(IWebElement conversationListItem);
        bool ClickConverstaionListItem(IWebElement element);
        bool WaitUntilConversationIsDisplayed(IWebElement messageBubble, IWebDriver webDriver);
        IList<IWebElement> GetMessageContents(IWebElement conversationPopUp);
        string GetProspectNameFromConversationItem(IWebElement conversationListItem);
        IList<IWebElement> GetOpenConversationItems(IWebDriver webDriver);
        string GetProspectNameFromMessageBubble(IWebElement messageBubbleListItem);
        bool IsMinimized(IWebElement conversationItem);
        string GetProspectNameFromMinimizedConversationItem(IWebElement conversationListItem);
        bool ClickMinimizedConversation(IWebElement conversationListItem);
    }
}
