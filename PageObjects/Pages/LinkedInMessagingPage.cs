using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.LinkedInPages;
using Leadsly.Application.Model.LinkedInPages.Interface;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInMessagingPage : LeadslyBase, ILinkedInMessagingPage
    {
        public LinkedInMessagingPage(ILogger<LinkedInMessagingPage> logger, IHumanBehaviorService humanBehaviorService) : base(logger)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<LinkedInMessagingPage> _logger;


        public HalOperationResult<T> ClickCreateNewMessage<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> ClickSend<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> EnterMessageContent<T>(IWebDriver webDriver, string messageContent) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> EnterProspectsName<T>(IWebDriver webDriver, string name) where T : IOperationResponse
        {
            throw new NotImplementedException();
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
            if(conversationListItems == null)
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
            catch(Exception ex)
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


        private IWebElement SearchMessagesInputField(IWebDriver webDriver)
        {
            IWebElement searchMessagesInput = default;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));                
                bool searchTermVisible = wait.Until((drv) => 
                {
                    searchMessagesInput = drv.FindElement(By.CssSelector("input[name='searchTerm']"));
                    return searchMessagesInput != null;
                });               

                if(searchTermVisible == false)
                {
                    _logger.LogError("Unable to locate 'search messages' input field after the given wait time");
                }
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
            if(SearchMessagesInputField == null)
            {
                return result;
            }

            try
            {
                _humanBehaviorService.EnterValues(searchMessagesInputField, searchCriteria, 200, 300);

                // verify that the entered string matches the search criteria
                string enteredValue = searchMessagesInputField.GetAttribute("value");
                if(enteredValue != searchCriteria)
                {
                    _logger.LogError("WebDriver did not enter in search criteria value into the search input field correctly. Entered value did not match search criteria value. " +
                        "\r\nEntered value is {enteredValue}" +
                        "\r\nSearch criteria value {searchCriteria}", enteredValue, searchCriteria);

                    return result;
                }

                searchMessagesInputField.SendKeys(Keys.Enter);

            }
            catch(Exception ex)
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
            if(messagesDivs == null)
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

        public string GetMessageContent(IWebElement message)
        {
            string content = string.Empty;
            try
            {
                IWebElement messagePTag = message.FindElement(By.CssSelector(".msg-s-event-listitem__body"));
                content = messagePTag.Text;
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

        public HalOperationResult<T> ClearMessagingSearchCriteria<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            _logger.LogInformation("Clearing 'search term' input field");
            HalOperationResult<T> result = new();
            try
            {
                IWebElement searchResultCriteria = SearchMessagesInputField(webDriver);
                if(searchResultCriteria == null)
                {
                    return result;
                }

                RandomWait(1, 2);
                searchResultCriteria.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear messaging search criteria value");
                return result;
            }

            result.Succeeded = true;
            return result;
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
                _logger.LogInformation(ex, "Converstaion list item does not contain notifications. This list item does not have any unread messages");
            }

            return hasNotification;
        }
    }
}
