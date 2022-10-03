using Domain.POMs.Controls;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Controls
{
    public class MessageListBubble : IMessageListBubble
    {
        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly ILogger<MessageListBubble> _logger;

        public MessageListBubble(
            ILogger<MessageListBubble> logger,
            IWebDriverUtilities webDriverUtilities)
        {
            _webDriverUtilities = webDriverUtilities;
            _logger = logger;
        }

        private const string MessageOverlay_CssLocator = "#msg-overlay .msg-overlay-list-bubble";
        private const string MessageConversationListItem_ClassNameLocator = "msg-conversation-listitem__link";
        private const string NewNotification_ClassNameLocator = "notification-badge--show";
        private const string ConversationWrapper_ClassNameLocator = "msg-convo-wrapper";
        private const string ConversationItemParagraphs_CssLocator = ".msg-s-event__content > p";
        private const string ConversationListItems_ClassNameLocator = "msg-s-event-listitem";
        private const string OpenedConversationItemProspectName_CssLocator = ".msg-overlay-conversation-bubble-header--fade-in h2 span";
        private const string MinimizedConversationItemProspectName_CssLocator = ".msg-overlay-conversation-bubble-header--fade-in h2";
        private const string InactiveMessageWindow_ClassNameLocator = "msg-overlay-conversation-bubble--default-inactive";
        private const string BubbleMessageListItemProspectName_CssLocator = ".msg-conversation-listitem__link h3";
        private const string ConversationPopUpInput_ClassNameSelector = "msg-form__contenteditable";
        private const string SendMessageButton_ClassNameSelector = "msg-form__send-button";

        private IWebElement? MessageListBubblesElement { get; set; }

        public bool MessageListBubblesControl(IWebDriver webDriver)
        {
            IWebElement messageListContainer = MessageOverlayListBubble(webDriver);

            if (messageListContainer == null)
            {
                return false;
            }

            MessageListBubblesElement = messageListContainer;
            return true;
        }

        private IWebElement MessageOverlayListBubble(IWebDriver webDriver)
        {
            IWebElement messageOverlayListBubble = default;

            try
            {
                webDriver.SwitchTo().DefaultContent();
                messageOverlayListBubble = webDriver.FindElement(By.CssSelector(MessageOverlay_CssLocator));
                webDriver.SwitchTo().DefaultContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate message overlay list bubble control by css selector {0}", MessageOverlay_CssLocator);
            }

            return messageOverlayListBubble;
        }

        public IList<IWebElement> GetAllMessagesListBubbles(IWebDriver webDriver)
        {
            if (MessageListBubblesElement == null)
            {
                _logger.LogError("{0} should not have been null. If we are here this means previous locating operation succeeded. Something is wrong", nameof(MessageListBubblesElement));
                return null;
            }

            return ConversationListItems(webDriver);
        }

        private IList<IWebElement> ConversationListItems(IWebDriver webDriver)
        {
            IList<IWebElement> converstaionListItems = default;
            try
            {
                IReadOnlyCollection<IWebElement> collection = MessageListBubblesElement.FindElements(By.ClassName(MessageConversationListItem_ClassNameLocator));
                converstaionListItems = collection.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate conversation list items for the message bubble list control");
            }

            return converstaionListItems;
        }

        public bool UnreadMessageNotificationExists(IWebElement conversationListItem)
        {
            bool newNotificationExists = false;
            try
            {
                IWebElement newNotification = conversationListItem.FindElement(By.ClassName(NewNotification_ClassNameLocator));
                newNotificationExists = newNotification != null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("New notification does not exist for this web element");
            }

            return newNotificationExists;
        }

        public bool ClickConverstaionListItem(IWebElement element)
        {
            bool succeeded = false;
            try
            {
                _logger.LogInformation("Clicking conversation list item in the /invite-connect/connections/ page to bring the conversation into the window");
                element.Click();
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click ConversationListItem");
            }
            return succeeded;
        }

        public IList<IWebElement> GetOpenConversationItems(IWebDriver webDriver)
        {
            IList<IWebElement> conversationWrappers = _webDriverUtilities.WaitUntilNotNull(ConversationWrappers, webDriver, 15);

            return conversationWrappers;
        }

        public bool WaitUntilConversationIsDisplayed(IWebElement messageBubble, IWebDriver webDriver)
        {
            IList<IWebElement> conversationWrappers = _webDriverUtilities.WaitUntilNotNull(ConversationWrappers, webDriver, 15);

            return conversationWrappers != null;
        }

        private IList<IWebElement> ConversationWrappers(IWebDriver webDriver)
        {
            IList<IWebElement> conversationWrappers = default;
            try
            {
                webDriver.SwitchTo().DefaultContent();
                conversationWrappers = webDriver.FindElements(By.ClassName(ConversationWrapper_ClassNameLocator));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Conversation has not yet been opened");
            }

            return conversationWrappers;
        }

        public IList<IWebElement> GetMessageContents(IWebElement conversationPopUp)
        {
            IReadOnlyCollection<IWebElement> messagesDivs = MessagesPs(conversationPopUp);
            if (messagesDivs == null)
            {
                _logger.LogDebug("Failed to retrieve contents of the currently active message");
                return new List<IWebElement>();
            }
            _logger.LogDebug("Retrieved contents of the currently active message");
            return messagesDivs.ToList();
        }

        public IList<IWebElement> GetMessageListItems(IWebElement conversationPopUp)
        {
            IReadOnlyCollection<IWebElement> messageListItemsElements = MessagesListItems(conversationPopUp);
            if (messageListItemsElements == null)
            {
                _logger.LogDebug("Failed to retrieve contents of the currently active message");
                return new List<IWebElement>();
            }
            _logger.LogDebug("Retrieved contents of the currently active message");
            return messageListItemsElements.ToList();
        }

        private IReadOnlyCollection<IWebElement> MessagesPs(IWebElement conversationPopUp)
        {
            IReadOnlyCollection<IWebElement> paragraphs = default;
            try
            {
                paragraphs = conversationPopUp.FindElements(By.CssSelector(ConversationItemParagraphs_CssLocator));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate any paragraphs for this conversation");
            }
            return paragraphs;
        }

        private IReadOnlyCollection<IWebElement> MessagesListItems(IWebElement conversationPopUp)
        {
            IReadOnlyCollection<IWebElement> paragraphs = default;
            try
            {
                paragraphs = conversationPopUp.FindElements(By.CssSelector(".msg-s-event-listitem"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate any paragraphs for this conversation");
            }
            return paragraphs;
        }

        public string GetProspectNameFromConversationPopup(IWebElement conversationPopUp)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement spanElement = conversationPopUp.FindElement(By.CssSelector(OpenedConversationItemProspectName_CssLocator));
                prospectName = spanElement.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract prospects name from conversation list item");
            }
            return prospectName;
        }

        public string GetProspectNameFromConversationListItem(IWebElement conversationListItem)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement prospectNameSpan = conversationListItem.FindElement(By.ClassName("msg-s-message-group__profile-link"));
                prospectName = prospectNameSpan?.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to locate prospects name from conversation list item");
            }

            return prospectName;
        }

        public string GetProspectNameFromMinimizedConversationItem(IWebElement conversationListItem)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement minimizedProspectElement = conversationListItem.FindElement(By.ClassName(MinimizedConversationItemProspectName_CssLocator));
                prospectName = minimizedProspectElement.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract prospects name from conversation list item");
            }
            return prospectName;
        }

        public string GetProspectNameFromMessageBubble(IWebElement messageBubbleListItem)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement h3 = messageBubbleListItem.FindElement(By.CssSelector(BubbleMessageListItemProspectName_CssLocator));

                prospectName = h3.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate clicked prospect name. Searched by {0}", BubbleMessageListItemProspectName_CssLocator);
            }

            return prospectName;
        }

        public bool IsMinimized(IWebElement conversationItem)
        {
            bool isMinimized = false;
            try
            {
                IWebElement minimizedElement = conversationItem.FindElement(By.ClassName(InactiveMessageWindow_ClassNameLocator));
                isMinimized = true;
            }
            catch (Exception)
            {
                _logger.LogDebug("Conversation list item is active and opened. This means it is NOT minimized");
            }

            return isMinimized;
        }

        public bool ClickMinimizedConversation(IWebElement conversationListItem)
        {
            bool succeeded = _webDriverUtilities.HandleClickElement(conversationListItem);

            return succeeded;
        }

        public IWebElement GetEnterMessageInputField(IWebDriver webDriver, IWebElement conversationPopUp)
        {
            IWebElement inputDiv = EnterMessageInputDiv(conversationPopUp);

            return inputDiv;
        }

        private IWebElement EnterMessageInputDiv(IWebElement conversationPopup)
        {
            IWebElement div = default;
            try
            {
                div = conversationPopup.FindElement(By.ClassName(ConversationPopUpInput_ClassNameSelector));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate the input div for the conversation popup");
            }

            return div;
        }

        public bool ClickSendMessage(IWebDriver webDriver, IWebElement conversationPopUp)
        {
            IWebElement button = _webDriverUtilities.WaitUntilNotNull(SendButton, webDriver, 10);
            if (button == null)
            {
                return false;
            }

            if (button.Enabled == false)
            {
                _logger.LogError("The send button is disabled. This means we cannot interact with it. Was the follow up message entered?");
                return false;
            }

            return _webDriverUtilities.HandleClickElement(button);
        }

        private IWebElement SendButton(IWebDriver webDriver)
        {
            IWebElement button = default;
            try
            {
                button = webDriver.FindElement(By.ClassName(SendMessageButton_ClassNameSelector));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Could not locate the 'Send' button to send follow up message");
            }
            return button;
        }

    }
}
