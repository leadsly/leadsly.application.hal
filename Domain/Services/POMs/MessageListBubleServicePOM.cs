using Domain.POMs.Controls;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.POMs
{
    public class MessageListBubbleServicePOM : IMessageListBubbleServicePOM
    {
        private readonly ILogger<MessageListBubbleServicePOM> _logger;
        private readonly IMessageListBubble _pom;
        private readonly IHumanBehaviorService _humanBehaviorService;

        public MessageListBubbleServicePOM(
            ILogger<MessageListBubbleServicePOM> logger,
            IMessageListBubble pom,
            IHumanBehaviorService humanBehaviorService
            )
        {
            _logger = logger;
            _pom = pom;
            _humanBehaviorService = humanBehaviorService;
        }

        public bool ClickNewMessage(IWebElement newMessageListItem, IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(1000, 1500);
            if (_pom.ClickConverstaionListItem(newMessageListItem) == false)
            {
                return false;
            }
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1300);

            IList<IWebElement> openedConversations = _pom.GetOpenConversationItems(webDriver);

            if (openedConversations == null)
            {
                _logger.LogWarning("Failed to get opened conversations.");
                return false;
            }

            if (openedConversations.Count == 0)
            {
                _logger.LogWarning("There are no open conversations");
            }

            if (openedConversations.Count > 1)
            {
                string clickedProspectName = _pom.GetProspectNameFromMessageBubble(newMessageListItem);
                _logger.LogDebug("Multiple opened conversations detected. Searching for the one that was clicked. Prospect name that was clicked {0}", clickedProspectName);

                foreach (IWebElement openedConversation in openedConversations)
                {
                    if (_pom.IsMinimized(openedConversation) == true)
                    {
                        string prospectName = _pom.GetProspectNameFromMinimizedConversationItem(openedConversation);

                        if (prospectName == clickedProspectName)
                        {
                            // we need to ensure this becomes the active conversation
                            _pom.ClickMinimizedConversation(openedConversation);
                            _humanBehaviorService.RandomWaitMilliSeconds(800, 1320);
                        }
                    }
                    else
                    {
                        string prospectName = _pom.GetProspectNameFromConversationItem(openedConversation);
                        if (prospectName == clickedProspectName)
                        {
                            // this means we've already have the current conversation active
                            break;
                        }
                    }
                }
            }

            // wait until the conversation is opened
            if (_pom.WaitUntilConversationIsDisplayed(newMessageListItem, webDriver) == false)
            {
                _logger.LogWarning("Could not locate conversation dialog after clicking the message list item. It's possible there was a misfire click that occured and the conversation was never opened");
                return false;
            }

            return true;
        }

        public string GetMessageContent(IWebElement conversationPopUp)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1200);
            string content = string.Empty;
            IList<IWebElement> activeMessageContents = _pom.GetMessageContents(conversationPopUp);
            if (activeMessageContents == null)
            {
                return content;
            }

            try
            {
                IWebElement lastMessage = activeMessageContents.LastOrDefault();
                if (lastMessage == null)
                {
                    _logger.LogWarning("Could not retrieve last message in the messages list");
                    return content;
                }

                content = lastMessage.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active pop up message contents");
            }

            return content;
        }

        public IList<IWebElement> GetMessagesListBubbles(IWebDriver webDriver)
        {
            if (_pom.MessageListBubblesControl(webDriver) == false)
            {
                _logger.LogWarning("Failed to locate message list bubble control");
                return null;
            }

            IList<IWebElement> allMessageListBubbles = _pom.GetAllMessagesListBubbles(webDriver);

            if (allMessageListBubbles == null)
            {
                _logger.LogWarning("Failed to all message list items from the messages bubble list");
                return null;
            }

            return allMessageListBubbles;
        }

        public IList<IWebElement> GetUnreadMessagesListBubbles(IList<IWebElement> messageListBubbles)
        {
            // filter down the list to only those prospecst that have the blue notification icon
            IList<IWebElement> unreadMessages = default;
            try
            {
                unreadMessages = messageListBubbles.Where(x => _pom.UnreadMessageNotificationExists(x)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Operation failed to filter down the list to only those messages that have been unread. The operation was looking for the blue icon inside the message bubble item");
            }

            return unreadMessages;
        }

        public string ProspectNameFromMessage(IWebElement element)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 900);
            return _pom.GetProspectNameFromConversationItem(element);
        }
    }
}
