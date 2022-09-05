using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Pages
{
    public interface ILinkedInMessagingPage
    {
        bool ClickCreateNewMessage(IWebDriver webDriver);

        IWebElement NewMessageNameInput(IWebDriver webDriver);

        bool ClickWriteAMessageBox(IWebDriver webDriver);

        IWebElement GetWriteAMessagePTag(IWebDriver webDriver);

        bool ClickSendMessage(IWebDriver webDriver);

        IList<IWebElement> GetVisibleConversationListItems(IWebDriver webDriver);

        string GetProspectNameFromConversationItem(IWebElement conversationListItem);

        bool IsNoMessagesDisplayed(IWebDriver webDriver);

        bool IsConversationListItemActive(IWebElement conversationListItem);

        bool ClickConverstaionListItem(IWebElement element, IWebDriver webDriver);

        IWebElement SearchMessagesInputField(IWebDriver webDriver);

        IList<IWebElement> GetMessageContents(IWebDriver webDriver);

        string GetProspectNameFromMessageContentPTag(IWebElement messageDiv);

        bool ClearMessagingSearchCriteria(IWebDriver webDriver);

        bool HasNotification(IWebElement listItem);

        bool IsActiveMessageItem(IWebElement listItem);
        bool NewMessageLabel(IWebDriver webDriver);

        IWebElement MessagingHeader(IWebDriver webDriver);
    }
}
