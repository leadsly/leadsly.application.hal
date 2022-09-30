using Domain.POMs;
using Domain.POMs.Controls;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.POMs
{
    public class FollowUpMessageOnConnectionsServicePOM : IFollowUpMessageOnConnectionsServicePOM
    {
        public FollowUpMessageOnConnectionsServicePOM(
            ILogger<FollowUpMessageOnConnectionsServicePOM> logger,
            IHumanBehaviorService humanBehaviorService,
            IMessageListBubble messageBubblePOM,
            IConnectionsView connectionsView)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _messageBubblePOM = messageBubblePOM;
            _connectionsView = connectionsView;
        }

        private readonly ILogger<FollowUpMessageOnConnectionsServicePOM> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IMessageListBubble _messageBubblePOM;
        private readonly IConnectionsView _connectionsView;

        public IList<IWebElement> Messages { get; private set; }

        public IWebElement GetProspectFromRecentlyAdded(IWebDriver webDriver, string prospectName)
        {
            IList<IWebElement> recentlyAdded = _connectionsView.GetRecentlyAdded(webDriver);
            if (recentlyAdded == null)
            {
                _logger.LogError("Could not get recently added list");
                return null;
            }

            if (recentlyAdded.Any(x => _connectionsView.GetNameFromLiTag(x) == prospectName) == true)
            {
                // scroll prospect into view
                IWebElement prospectFound = recentlyAdded.First(x => _connectionsView.GetNameFromLiTag(x) == prospectName);

                if (webDriver.IsElementVisible(prospectFound) == false)
                {
                    webDriver.ScrollIntoView(prospectFound);
                }

                return prospectFound;
            }
            else
            {
                return null;
            }
        }

        public bool ClickMessageProspect(IWebDriver webDriver, IWebElement prospect)
        {
            if (webDriver.IsElementVisible(prospect) == false)
            {
                webDriver.ScrollIntoView(prospect);
            }

            return _connectionsView.ClickMessage(prospect);
        }

        public bool? DoesLastMessageMatchPreviouslySentMessage(IWebElement lastMessage, string previousMessageContent)
        {
            string lastMessageContent = lastMessage.Text;
            if (string.IsNullOrEmpty(lastMessageContent) == false)
            {
                return null;
            }

            return lastMessageContent == previousMessageContent;
        }

        public bool? WasLastMessageSentByProspect(IWebElement lastMessage, string prospectName)
        {
            string messageSentBy = _messageBubblePOM.GetProspectNameFromConversationItem(lastMessage);
            if (string.IsNullOrEmpty(messageSentBy) == true)
            {
                _logger.LogWarning("Could not determine who sent the last message");
                return null;
            }

            return messageSentBy == prospectName;
        }

        public bool IsThereConversationHistory(IWebElement conversation)
        {
            Messages = _messageBubblePOM.GetMessageContents(conversation);
            return Messages.Count != 0;
        }

        public IWebElement GetPopUpConversation(IWebDriver webDriver, string prospectNameIn)
        {
            IWebElement openedConversationPopUp = default;
            IList<IWebElement> popupConversations = _messageBubblePOM.GetOpenConversationItems(webDriver);
            if (popupConversations == null || popupConversations.Count == 0)
            {
                return null;
            }

            foreach (IWebElement popUpConversation in popupConversations)
            {
                if (_messageBubblePOM.IsMinimized(popUpConversation) == true)
                {
                    string prospectName = _messageBubblePOM.GetProspectNameFromMinimizedConversationItem(popUpConversation);

                    if (prospectName == prospectNameIn)
                    {
                        // we need to ensure this becomes the active conversation
                        _messageBubblePOM.ClickMinimizedConversation(popUpConversation);
                        _humanBehaviorService.RandomWaitMilliSeconds(800, 1320);
                        openedConversationPopUp = popUpConversation;
                    }
                }
                else
                {
                    string prospectName = _messageBubblePOM.GetProspectNameFromConversationItem(popUpConversation);
                    if (prospectName == prospectNameIn)
                    {
                        // this means we've already have the current conversation active
                        openedConversationPopUp = popUpConversation;
                        break;
                    }
                }
            }

            return openedConversationPopUp;
        }

        public bool EnterMessage(IWebDriver webDriver, IWebElement conversationPopUp, string content)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(300, 500);

            IWebElement messageInputField = _messageBubblePOM.GetEnterMessageInputField(webDriver, conversationPopUp);
            if (messageInputField == null)
            {
                return false;
            }

            foreach (char character in content)
            {
                _humanBehaviorService.EnterValue(messageInputField, character, 150, 300);
            }

            bool sendMessageClick = _messageBubblePOM.ClickSendMessage(webDriver, conversationPopUp);
            if (sendMessageClick == false)
            {
                return false;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(400, 750);

            return true;
        }
    }
}
