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

        public IWebElement GetProspectFromRecentlyAdded(IWebDriver webDriver, string prospectName, string prospectProfileUrl, bool isListFiltered)
        {
            IList<IWebElement> recentlyAdded = default;
            if (isListFiltered == true)
            {
                recentlyAdded = _connectionsView.GetRecentlyAddedFiltered(webDriver);
            }
            else
            {
                recentlyAdded = _connectionsView.GetRecentlyAdded(webDriver);
            }

            if (recentlyAdded == null)
            {
                _logger.LogError("Could not get recently added list");
                return null;
            }

            if (recentlyAdded.Any(x => _connectionsView.GetNameFromLiTag(x).Contains(prospectName)) == true)
            {
                // scroll prospect into view
                IWebElement prospectFound = default;
                if (recentlyAdded.Count(x => _connectionsView.GetNameFromLiTag(x).Contains(prospectName)) > 1)
                {
                    // we have multiple results. Lets compare linkedin profiles to ensure we're getting the right one
                    prospectFound = recentlyAdded.Where(x => _connectionsView.GetNameFromLiTag(x).Contains(prospectName)).FirstOrDefault(x => _connectionsView.GetProfileUrlFromLiTag(x).Contains(prospectProfileUrl));
                }
                else
                {
                    prospectFound = recentlyAdded.First(x => _connectionsView.GetNameFromLiTag(x) == prospectName);
                }

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
            _humanBehaviorService.RandomWaitMilliSeconds(600, 1030);
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

                    if (prospectName.Contains(prospectNameIn))
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
                    if (prospectName.Contains(prospectNameIn))
                    {
                        // this means we've already have the current conversation active
                        openedConversationPopUp = popUpConversation;
                        break;
                    }
                }
            }

            return openedConversationPopUp;
        }

        public bool SendMessage(IWebDriver webDriver, IWebElement conversationPopUp, string content)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(300, 3500);

            IWebElement messageInputField = _messageBubblePOM.GetEnterMessageInputField(webDriver, conversationPopUp);
            if (messageInputField == null)
            {
                return false;
            }

            foreach (char character in content)
            {
                _humanBehaviorService.EnterValue(messageInputField, character, 150, 400);
            }

            bool sendMessageClick = _messageBubblePOM.ClickSendMessage(webDriver, conversationPopUp);
            if (sendMessageClick == false)
            {
                return false;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(400, 750);

            return true;
        }

        public bool? EnterProspectName(IWebDriver webDriver, string prospectName)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(450, 650);
            IWebElement inputField = _connectionsView.GetProspectSearchInputField(webDriver);
            if (inputField == null)
            {
                return null;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(700, 950);
            _connectionsView.ClickProspectSearchInputField(webDriver);

            foreach (char character in prospectName)
            {
                _humanBehaviorService.EnterValue(inputField, character, 150, 400);
            }

            // just ensure the name entered is the expected name
            string actualValue = inputField.GetAttribute("value");

            // this could mean that linkedin has removed spaces for us. Clear the field and try again
            if (actualValue.Contains(' ') == false)
            {
                // add a space between first and last name
            }

            if (actualValue != prospectName)
            {
                _logger.LogWarning("The prospect name that was entered {0}, did not equal to what the input field received {1}", prospectName, actualValue);
                return false;
            }

            return true;
        }

        public bool ClearProspectFilterInputField(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1950);

            IWebElement inputField = _connectionsView.GetProspectSearchInputField(webDriver);
            if (inputField == null)
            {
                return false;
            }

            bool succeeded = false;
            try
            {
                // use backspace sometimes, or ctrl + a + del

                inputField.Clear();
                succeeded = true;
            }
            catch
            {
                _logger.LogDebug("Failed to clear prospect filter input field");
            }

            return succeeded;
        }
    }
}
