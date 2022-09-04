using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.LinkedInPages;
using Leadsly.Application.Model.LinkedInPages.Interface;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Pages
{
    public class LinkedInMessagingPage : LeadslyBase, ILinkedInMessagingPage
    {
        public LinkedInMessagingPage(
            ILogger<LinkedInMessagingPage> logger,
            IHumanBehaviorService humanBehaviorService,
            IWebDriverUtilities webDriverUtilities
            ) : base(logger)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<LinkedInMessagingPage> _logger;

        private IWebElement ComposeNewMessageAnchorTag(IWebDriver webDriver)
        {
            IWebElement composeNewMsgAnchor = default;
            try
            {
                composeNewMsgAnchor = webDriver.FindElement(By.ClassName("msg-conversations-container__compose-btn"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to locate new message compose button by class name 'msg-conversations-container__compose-btn'");
            }
            return composeNewMsgAnchor;
        }


        public HalOperationResult<T> ClickCreateNewMessage<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement composeMsg = default;
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            wait.Until((drv) =>
            {
                composeMsg = ComposeNewMessageAnchorTag(drv);
                return composeMsg != null;
            });

            if (composeMsg == null)
            {
                _logger.LogInformation("Compose new message button is null");
                return result;
            }

            _logger.LogInformation("Clicking compose new message button");
            composeMsg.Click();

            result.Succeeded = true;
            return result;
        }

        public bool ClickCreateNewMessage(IWebDriver webDriver)
        {
            IWebElement composeMessage = _webDriverUtilities.WaitUntilNotNull(ComposeNewMessageAnchorTag, webDriver, 30);
            if (composeMessage == null)
            {
                return false;
            }

            return _webDriverUtilities.HandleClickElement(composeMessage);
        }


        private IWebElement SendNewMessageButton(IWebDriver webDriver)
        {
            IWebElement btn = default;
            try
            {
                btn = webDriver.FindElement(By.ClassName("msg-form__send-button"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate the 'Send' button by class name 'msg-form__send-button'");
            }

            return btn;
        }

        private void WaitUntilSendButtonIsEnabled(IWebDriver webDriver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                wait.Until((drv) =>
                {
                    IWebElement sendButton = SendNewMessageButton(drv);
                    return sendButton != null && sendButton.Enabled == true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate send new message button");
            }
        }

        public HalOperationResult<T> ClickSend<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            WaitUntilSendButtonIsEnabled(webDriver);
            IWebElement sendButton = SendNewMessageButton(webDriver);
            if (sendButton == null)
            {
                return result;
            }

            sendButton.Click();

            result.Succeeded = true;
            return result;
        }

        public bool ClickSendMessage(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement sendButton = _webDriverUtilities.WaitUntilNotNull(SendNewMessageButton, webDriver, 30);
            if (sendButton == null)
            {
                _logger.LogDebug("Send button was not located");
                return succeeded;
            }

            return _webDriverUtilities.HandleClickElement(sendButton);

        }

        private IWebElement ContentEditableDiv(IWebDriver webDriver)
        {
            IWebElement contentEditableDiv = default;
            try
            {
                contentEditableDiv = webDriver.FindElement(By.ClassName("msg-form__contenteditable"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not locate div that houses p tag with placeholder text 'Write a message'");
            }
            return contentEditableDiv;
        }

        public IWebElement GetWriteAMessagePTag(IWebDriver webDriver)
        {
            IWebElement pTag = default;
            try
            {
                IWebElement contentEditableDiv = _webDriverUtilities.WaitUntilNull(ContentEditableDiv, webDriver, 30);
                if (contentEditableDiv == null)
                {
                    return null;
                }

                pTag = contentEditableDiv.FindElement(By.TagName("p"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Unable to find p tag with 'Write a message' text");
            }
            return pTag;
        }

        public HalOperationResult<T> EnterMessageContent<T>(IWebDriver webDriver, string messageContent) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement pTagInsideContent = GetWriteAMessagePTag(webDriver);
            if (pTagInsideContent == null)
            {
                return result;
            }

            _humanBehaviorService.EnterValues(pTagInsideContent, messageContent, 100, 150);

            string enteredValue = pTagInsideContent.Text;
            if (enteredValue != messageContent)
            {
                _logger.LogError("The messaged entered into the p tag did not match exactly." +
                    "\r\nThe expected message {messageContent}" +
                    "\r\nEntered value {enteredValue}", messageContent, enteredValue);
            }

            result.Succeeded = true;
            return result;
        }

        public IWebElement NewMessageNameInput(IWebDriver webDriver)
        {
            IWebElement inputField = default;
            try
            {
                inputField = webDriver.FindElement(By.ClassName("msg-connections-typeahead__search-field--no-recipients"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to locate new message input field");
            }

            return inputField;
        }

        public HalOperationResult<T> EnterProspectsName<T>(IWebDriver webDriver, string name) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                IWebElement inputField = NewMessageNameInput(webDriver);
                if (inputField == null)
                {
                    return result;
                }

                _humanBehaviorService.EnterValues(inputField, name, 250, 400);

                // verify that the name was entered in correctly 
                string enteredValue = inputField.GetAttribute("value");
                if (enteredValue != name)
                {
                    _logger.LogError("Comparison between what value was entered into the new message input field and prospect's name was not exactly the same." +
                        "\r\nThis means some or all values were not correctly entered into the input field");

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send keys after clicking compose new message button");
            }

            result.Succeeded = true;
            return result;
        }

        public bool EnterProspectName(IWebDriver webDriver, string name)
        {

        }

        private IWebElement TypeAheadSearchResultsExpandedContainer(IWebDriver webDriver)
        {
            IWebElement container = default;
            try
            {
                container = webDriver.FindElement(By.ClassName("msg-connections-typeahead__search-results--expanded"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate type ahead prospect container by class name 'msg-connections-typeahead__search-results--expanded'");
            }

            return container;
        }

        private IWebElement WaitUntilTypeAheadResultsIsVisible(IWebDriver webDriver)
        {
            IWebElement visibleTypeAhead = default;
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
            try
            {
                wait.Until(drv =>
                {
                    visibleTypeAhead = TypeAheadSearchResultsExpandedContainer(drv);
                    return visibleTypeAhead != null && visibleTypeAhead.Displayed;
                });
            }
            catch (Exception ex)
            {

            }

            return visibleTypeAhead;
        }

        private IWebElement ResultsTypeAheadLoader(IWebDriver webDriver)
        {
            IWebElement loader = default;
            try
            {
                loader = webDriver.FindElement(By.CssSelector(".msg-connections-typeahead__margin-transition-in .artdeco-loader--small"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Type ahead results loader not found. This is ok since the loader only appears when some data is fetched");
            }
            return loader;
        }

        public HalOperationResult<T> ConfirmProspectName<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement inputField = NewMessageNameInput(webDriver);
            inputField.Click();

            // grab the expanded type ahead container
            IWebElement visibleTypeAheadResultsContainer = WaitUntilTypeAheadResultsIsVisible(webDriver);

            if (visibleTypeAheadResultsContainer == null)
            {
                return result;
            }

            // wait until the container is not loading
            WebDriverWait wait = new(webDriver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(drv =>
                {
                    return ResultsTypeAheadLoader(drv) == null;
                });
            }
            catch (Exception ex)
            {
                return result;
            }

            inputField.SendKeys(Keys.Enter);

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> ClickWriteAMessageBox<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement pTagInsideContent = GetWriteAMessagePTag(webDriver);
            if (pTagInsideContent == null)
            {
                return result;
            }

            pTagInsideContent.Click();
            _logger.LogInformation("Clicked 'Write a message...' p tag");

            result.Succeeded = true;
            return result;
        }

        public bool ClickWriteAMessageBox(IWebDriver webDriver)
        {
            IWebElement writeMessageElement = GetWriteAMessagePTag(webDriver);
            if (writeMessageElement == null)
            {
                return false;
            }

            return _webDriverUtilities.HandleClickElement(writeMessageElement);
        }

        private IWebElement ConversationsListItemContainer(IWebDriver webDriver)
        {
            IWebElement conversationsListItemContainer = default;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15));
                wait.Until((drv) =>
                {
                    conversationsListItemContainer = drv.FindElement(By.CssSelector(".msg-conversations-container__conversations-list"));
                    return conversationsListItemContainer != null;
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate ConversationListItemContainer");
            }
            return conversationsListItemContainer;
        }

        private IReadOnlyCollection<IWebElement> GetConversationListItems(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> conversationListItems = default;
            try
            {
                conversationListItems = ConversationsListItemContainer(webDriver).FindElements(By.CssSelector("li.msg-conversation-listitem"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ConversationListItems");
            }
            return conversationListItems;
        }

        public HalOperationResult<T> GetVisibleConversationListItems<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IReadOnlyCollection<IWebElement> conversationListItems = GetConversationListItems(webDriver);
            if (conversationListItems == null)
            {
                _logger.LogError("Failed ");
                return result;
            }

            IScrapedHtmlElements elements = new ScrapedHtmlElements
            {
                HtmlElements = conversationListItems
            };

            result.Value = (T)elements;
            result.Succeeded = true;
            return result;
        }

        public IList<IWebElement> GetVisibleConversationListItems(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> conversationListItems = GetConversationListItems(webDriver);
            if (conversationListItems == null)
            {
                _logger.LogError("Failed to locate conversation list items");
                return null;
            }

            return conversationListItems.ToList();
        }

        public bool ConversationItemContainsNotificationBadge(IWebElement conversationListItem)
        {
            IWebElement notificationBadge = default;
            try
            {
                notificationBadge = conversationListItem.FindElement(By.CssSelector(".notification-badge--show"));
            }
            catch
            {
                _logger.LogInformation("The given ConversationListItem element does not contain notification badge icon");
            }

            return notificationBadge != null;
        }

        public string GetProspectNameFromConversationItem(IWebElement conversationListItem)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement h3Element = conversationListItem.FindElement(By.CssSelector(".msg-conversation-listitem__participant-names"));
                prospectName = h3Element.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract prospects name from conversation list item");
            }
            return prospectName;
        }

        private IWebElement NoMessagesPTag(IWebDriver webDriver)
        {
            IWebElement pTag = default;
            try
            {
                pTag = ConversationsListItemContainer(webDriver).FindElement(By.XPath("//p[text()[contains(., 'No messages')]]"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Couldn't find 'No messages' p tag element. This is probably OK, we could've been just checking if it was there or not.");
            }
            return pTag;
        }

        /// <summary>
        /// Checks whether the 'No Messages' text is displayed under the search messages input field. This means the provided
        /// search term was not found in our messages history.
        /// </summary>
        /// <param name="webDriver"></param>
        /// <returns></returns>
        public bool IsNoMessagesDisplayed(IWebDriver webDriver)
        {
            IWebElement conversationsContainer = NoMessagesPTag(webDriver);

            return conversationsContainer != null;
        }

        /// <summary>
        /// TODO this is not implemented
        /// </summary>
        /// <param name="conversationListItem"></param>
        /// <returns></returns>
        public string GetProspectProfileUrlFromConversationItem(IWebElement conversationListItem)
        {
            string prospectProfileUrl = string.Empty;
            try
            {

            }
            catch (Exception ex)
            {
            }

            return prospectProfileUrl;
        }

        public bool IsConversationListItemActive(IWebElement conversationListItem)
        {
            IWebElement activeElement = default;
            try
            {
                activeElement = conversationListItem.FindElement(By.CssSelector(".active"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate 'active' class on the ConversationListItem");
            }

            return activeElement != null;
        }

        public void ClickConverstaionListItem(IWebElement conversationListItem)
        {
            try
            {
                RandomWait(1, 2);
                _logger.LogInformation("Clicking conversation list item in the /messaging/ page to bring active conversation history in the right hand side view");
                conversationListItem.Click();
                RandomWait(1, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click ConversationListItem");
            }
        }

        public bool ClickConverstaionListItem(IWebElement conversationListItem, IWebDriver webDriver)
        {
            bool succeeded = false;
            try
            {
                _logger.LogInformation("Clicking conversation list item in the /messaging/ page to bring active conversation history in the right hand side view");
                conversationListItem.Click();
                if (WaitUntilConversationListItemIsActive(webDriver, conversationListItem) == false)
                {
                    _logger.LogDebug("Waited for conversation list item to become active, but it didn't");
                    succeeded = false;
                }
                else
                {
                    _logger.LogDebug("Conversation list item became active");
                    succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click ConversationListItem");
            }
            return succeeded;
        }

        private bool WaitUntilConversationListItemIsActive(IWebDriver webDriver, IWebElement prospectMessage)
        {
            bool active = false;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
                wait.Until(drv => IsConversationListItemActive(prospectMessage));
                active = true;
            }
            catch (Exception ex)
            {
                int timeout = 5;
                _logger.LogError(ex, "Waited for conversation list item to become active but it never did. Waited for {timeout}", timeout);
            }
            return active;
        }


        //private IWebElement SearchMessagesInputField(IWebDriver webDriver)
        //{
        //    IWebElement searchMessagesInput = default;
        //    try
        //    {
        //        WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
        //        bool searchTermVisible = wait.Until((drv) =>
        //        {
        //            searchMessagesInput = drv.FindElement(By.CssSelector("input[name='searchTerm']"));
        //            return searchMessagesInput != null;
        //        });

        //        if (searchTermVisible == false)
        //        {
        //            _logger.LogError("Unable to locate 'search messages' input field after the given wait time");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to locate 'search messages' input field");
        //    }
        //    return searchMessagesInput;
        //}

        public IWebElement SearchMessagesInputField(IWebDriver webDriver)
        {
            IWebElement input = _webDriverUtilities.WaitUntilNotNull(SearchMsgsInputField, webDriver, 5);
            return input;
        }

        private IWebElement SearchMsgsInputField(IWebDriver webDriver)
        {
            IWebElement searchMessagesInput = default;
            try
            {
                searchMessagesInput = webDriver.FindElement(By.CssSelector("input[name='searchTerm']"));
                _logger.LogDebug("Found 'search messages' input field");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate 'search messages' input field");
            }
            return searchMessagesInput;
        }

        public HalOperationResult<T> EnterSearchMessagesCriteria<T>(IWebDriver webDriver, string searchCriteria) where T : IOperationResponse
        {
            _logger.LogInformation("Entering search term into the input field");
            HalOperationResult<T> result = new();

            IWebElement searchMessagesInputField = SearchMessagesInputField(webDriver);
            if (SearchMessagesInputField == null)
            {
                return result;
            }

            try
            {
                _humanBehaviorService.EnterValues(searchMessagesInputField, searchCriteria, 200, 300);

                // verify that the entered string matches the search criteria
                string enteredValue = searchMessagesInputField.GetAttribute("value");
                if (enteredValue != searchCriteria)
                {
                    _logger.LogError("WebDriver did not enter in search criteria value into the search input field correctly. Entered value did not match search criteria value. " +
                        "\r\nEntered value is {enteredValue}" +
                        "\r\nSearch criteria value {searchCriteria}", enteredValue, searchCriteria);

                    return result;
                }

                searchMessagesInputField.SendKeys(Keys.Enter);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enter in prospects name into the search input field or to press enter");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement MessagesDetailSection(IWebDriver webDriver)
        {
            IWebElement messagesDetailSection = default;
            try
            {
                messagesDetailSection = webDriver.SwitchTo().DefaultContent().FindElement(By.CssSelector(".scaffold-layout__detail"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate MessagesDetailSection by the following class '.scaffold-layout__detail'");
            }
            return messagesDetailSection;
        }

        private IReadOnlyCollection<IWebElement> MessagesPs(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> messagesDivs = default;
            try
            {
                messagesDivs = MessagesDetailSection(webDriver).FindElements(By.CssSelector(".msg-s-event__content > p"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate messages divs by the following class '.msg-s-event-listitem'");
            }
            return messagesDivs;
        }

        public HalOperationResult<T> GetMessagesContent<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IReadOnlyCollection<IWebElement> messagesDivs = MessagesPs(webDriver);
            if (messagesDivs == null)
            {
                return result;
            }

            IScrapedHtmlElements elements = new ScrapedHtmlElements
            {
                HtmlElements = messagesDivs
            };

            result.Value = (T)elements;
            result.Succeeded = true;
            return result;
        }

        public IList<IWebElement> GetMessageContents(IWebDriver webDriver)
        {
            IList<IWebElement> messageContents = new List<IWebElement>();
            IReadOnlyCollection<IWebElement> messagesDivs = MessagesPs(webDriver);
            if (messagesDivs == null)
            {
                _logger.LogDebug("Failed to retrieve contents of the currently active message");
                return messageContents;
            }
            _logger.LogDebug("Retrieved contents of the currently active message");
            return messagesDivs.ToList();
        }

        public string GetMessageContent(IWebElement message)
        {
            string content = string.Empty;
            try
            {
                content = message.Text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract message content by class '.msg-s-event-listitem__body'");
            }

            return content;
        }

        public string GetProspectNameFromMessageDetailDiv(IWebElement messageDiv)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement span = messageDiv.FindElement(By.CssSelector(".msg-s-message-group__name"));
                prospectName = span.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract prospect name from the message div by class name '.msg-s-message-group__name'");
            }
            return prospectName;
        }

        public string GetProspectNameFromMessageContentPTag(IWebElement messagePTag)
        {
            _logger.LogDebug("Extracting prospect name from the message content p tag");
            string prospectName = string.Empty;
            try
            {
                IWebElement messageDetail = messagePTag.FindElement(By.XPath("./ancestor::div[contains(@class, 'msg-s-event-listitem')]"));
                IWebElement span = messageDetail.FindElement(By.CssSelector(".msg-s-message-group__name"));
                prospectName = span.Text;
                _logger.LogDebug("Extracted prospect name from the message content p tag {prospectname}", prospectName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract prospect name from the message div by class name '.msg-s-message-group__name'");
            }
            return prospectName;
        }

        public HalOperationResult<T> ClearMessagingSearchCriteria<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            _logger.LogInformation("Clearing 'search term' input field");
            HalOperationResult<T> result = new();
            try
            {
                IWebElement searchResultCriteria = SearchMessagesInputField(webDriver);
                if (searchResultCriteria == null)
                {
                    return result;
                }

                RandomWait(1, 2);
                searchResultCriteria.Clear();
                searchResultCriteria.SendKeys(Keys.Enter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear messaging search criteria value");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public bool ClearMessagingSearchCriteria(IWebDriver webDriver)
        {
            _logger.LogInformation("Clearing 'search term' input field");
            bool succeeded = false;
            try
            {
                IWebElement searchResultCriteria = SearchMessagesInputField(webDriver);
                if (searchResultCriteria == null)
                {
                    _logger.LogDebug("Failed to locate search result criteria input field");
                    succeeded = false;
                }
                else
                {
                    _logger.LogDebug("Located search result criteria input field");
                    searchResultCriteria.Clear();
                    searchResultCriteria.SendKeys(Keys.Enter);
                    succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear messaging search criteria value");
                succeeded = false;
            }

            return succeeded;
        }

        public bool HasNotification(IWebElement listItem)
        {
            bool hasNotification = false;
            try
            {
                IWebElement span = listItem.FindElement(By.ClassName("notification-badge--show"));
                hasNotification = span != null;
            }
            catch (Exception ex)
            {

            }

            return hasNotification;
        }

        public bool IsActiveMessageItem(IWebElement listItem)
        {
            IWebElement activeMessage = default;
            try
            {
                activeMessage = listItem.FindElement(By.CssSelector(".msg-conversations-container__convo-item-link.active"));
            }
            catch (Exception ex)
            {

            }
            return activeMessage != null;
        }

        public bool NewMessageLabel(IWebDriver webDriver)
        {
            IWebElement newMessageLabel = default;
            try
            {
                IWebElement messageDetailsSection = MessagesDetailSection(webDriver);
                newMessageLabel = messageDetailsSection.FindElement(By.ClassName("msg-s-message-list__new-message-heading"));

            }
            catch (Exception ex)
            {
            }

            return newMessageLabel != null;
        }

        private IWebElement MessagingH1Element(IWebDriver webDriver)
        {
            IWebElement header = default;
            try
            {
                header = webDriver.FindElement(By.CssSelector(".msg-conversations-container__title-row h1"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate messaging header. Attempted to locate it by '.msg-conversations-container__title-row h1' css selector");
            }
            return header;
        }

        public IWebElement MessagingHeader(IWebDriver webDriver)
        {
            IWebElement header = _webDriverUtilities.WaitUntilNull(MessagingH1Element, webDriver, 5);
            return header;
        }
    }
}
