using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Pages
{
    public interface ILinkedInMessagingPage
    {
        HalOperationResult<T> ClickCreateNewMessage<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> EnterProspectsName<T>(IWebDriver webDriver, string name)
            where T : IOperationResponse;

        HalOperationResult<T> ConfirmProspectName<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> ClickWriteAMessageBox<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> EnterMessageContent<T>(IWebDriver webDriver, string messageContent)
            where T : IOperationResponse;

        HalOperationResult<T> ClickSend<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> GetVisibleConversationListItems<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IList<IWebElement> GetVisibleConversationListItems(IWebDriver webDriver);

        bool ConversationItemContainsNotificationBadge(IWebElement conversationListItem);

        string GetProspectNameFromConversationItem(IWebElement conversationListItem);

        bool IsNoMessagesDisplayed(IWebDriver webDriver);
        string GetProspectProfileUrlFromConversationItem(IWebElement conversationListItem);

        bool IsConversationListItemActive(IWebElement conversationListItem);

        void ClickConverstaionListItem(IWebElement conversationListItem);

        bool ClickConverstaionListItem(IWebElement element, IWebDriver webDriver);

        IWebElement SearchMessagesInputField(IWebDriver webDriver);

        HalOperationResult<T> EnterSearchMessagesCriteria<T>(IWebDriver webDriver, string searchCriteria)
            where T : IOperationResponse;

        HalOperationResult<T> GetMessagesContent<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IList<IWebElement> GetMessageContents(IWebDriver webDriver);

        string GetMessageContent(IWebElement message);

        string GetProspectNameFromMessageDetailDiv(IWebElement messageDiv);
        string GetProspectNameFromMessageContentPTag(IWebElement messageDiv);

        HalOperationResult<T> ClearMessagingSearchCriteria<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        bool ClearMessagingSearchCriteria(IWebDriver webDriver);

        bool HasNotification(IWebElement listItem);

        IWebElement MessagingHeader(IWebDriver webDriver);
    }
}
