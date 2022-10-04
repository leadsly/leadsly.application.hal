using Domain.POMs;
using Domain.POMs.Controls;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
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
            IWebDriverProvider webDriverProvider,
            Random random,
            IConnectionsView connectionsView)
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _humanBehaviorService = humanBehaviorService;
            _messageBubblePOM = messageBubblePOM;
            _connectionsView = connectionsView;
            _rnd = random;
        }

        private readonly ILogger<FollowUpMessageOnConnectionsServicePOM> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IMessageListBubble _messageBubblePOM;
        private readonly IConnectionsView _connectionsView;
        private readonly Random _rnd;

        public IList<IWebElement> Messages { get; private set; }

        public IWebElement GetProspectFromRecentlyAdded(IWebDriver webDriver, string prospectName, string prospectProfileUrl, bool isListFiltered)
        {
            RecentlyAddedResults result = _connectionsView.DetermineRecentlyAddedResults(webDriver);

            if (result == RecentlyAddedResults.Unknown || result == RecentlyAddedResults.NoResults)
            {
                _logger.LogDebug("No results found in recently added list for prospect {0}.", prospectName);
                return null;
            }

            IList<IWebElement> recentlyAdded = isListFiltered ? _connectionsView.GetRecentlyAddedFiltered(webDriver) : _connectionsView.GetRecentlyAdded(webDriver);

            if (recentlyAdded == null)
            {
                _logger.LogError("Could not get recently added list");
                return null;
            }

            if (AreListElementsStale(recentlyAdded, webDriver) == true)
            {
                _humanBehaviorService.RandomWaitMilliSeconds(600, 960);
                recentlyAdded = isListFiltered ? _connectionsView.GetRecentlyAddedFiltered(webDriver) : _connectionsView.GetRecentlyAdded(webDriver);
            }

            if (recentlyAdded.Any(x => _connectionsView.GetNameFromLiTag(webDriver)?.RemoveEmojis() == prospectName) == true)
            {
                // scroll prospect into view
                IWebElement prospectFound = default;
                if (recentlyAdded.Count(x => _connectionsView.GetNameFromLiTag(webDriver)?.RemoveEmojis() == prospectName) > 1)
                {
                    // we have multiple results. Lets compare linkedin profiles to ensure we're getting the right one
                    prospectFound = recentlyAdded.Where(x => _connectionsView.GetNameFromLiTag(webDriver)?.RemoveEmojis() == prospectName).FirstOrDefault(x => _connectionsView.GetProfileUrlFromLiTag(x).Contains(prospectProfileUrl));
                }
                else
                {
                    prospectFound = recentlyAdded.First(x => _connectionsView.GetNameFromLiTag(webDriver)?.RemoveEmojis() == prospectName);
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

        private bool AreListElementsStale(IList<IWebElement> recentlyAdded, IWebDriver webDriver)
        {
            bool isStale = false;
            try
            {
                recentlyAdded.Select(x => _connectionsView.GetNameFromLiTag(webDriver));
            }
            catch (StaleElementReferenceException ex)
            {
                // re-grab the list
                isStale = true;
            }
            return isStale;
        }

        public bool ClickMessageProspect(IWebDriver webDriver, IWebElement prospect)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(1000, 2000);
            IWebElement connectionsHeader = _connectionsView.GetConnectionsHeader(webDriver);
            _humanBehaviorService.RandomClickElement(connectionsHeader);

            if (webDriver.IsElementVisible(prospect) == false)
            {
                webDriver.ScrollIntoView(prospect);
            }

            return _connectionsView.ClickMessage(prospect);
        }

        public bool? DoesLastMessageMatchPreviouslySentMessage(IWebElement lastMessage, string previousMessageContent)
        {
            IWebElement lastMessageP = LastMessagePElement(lastMessage);

            if (lastMessageP == null)
            {
                return null;
            }

            string lastMessageContent = lastMessageP.Text;
            if (string.IsNullOrEmpty(lastMessageContent) == true)
            {
                return null;
            }

            return lastMessageContent == previousMessageContent;
        }

        private IWebElement LastMessagePElement(IWebElement lastMessageListItem)
        {
            IWebElement lastMessagePElement = default;
            try
            {
                lastMessagePElement = lastMessageListItem.FindElement(By.CssSelector("p"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Unable to find last message p element");
            }

            return lastMessagePElement;
        }

        public bool? WasLastMessageSentByProspect(IWebElement lastMessage, string prospectName)
        {
            string messageSentBy = _messageBubblePOM.GetProspectNameFromConversationListItem(lastMessage);
            if (string.IsNullOrEmpty(messageSentBy) == true)
            {
                _logger.LogWarning("Could not determine who sent the last message");
                return null;
            }

            messageSentBy = messageSentBy.RemoveEmojis();

            return messageSentBy == prospectName;
        }

        public bool IsThereConversationHistory(IWebElement conversation)
        {
            Messages = _messageBubblePOM.GetMessageListItems(conversation);
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
                    string prospectName = _messageBubblePOM.GetProspectNameFromConversationPopup(popUpConversation);
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

            webDriver.HandleClickElement(messageInputField);
            _humanBehaviorService.RandomWaitMilliSeconds(800, 1050);

            foreach (char character in content)
            {
                _humanBehaviorService.EnterValue(messageInputField, character, 150, 400);
            }

            bool sendMessageClick = _messageBubblePOM.ClickSendMessage(webDriver, conversationPopUp);
            if (sendMessageClick == false)
            {
                return false;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(600, 3500);

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
                _humanBehaviorService.EnterValue(inputField, character, 150, 350);
            }

            // just ensure the name entered is the expected name
            string actualValue = inputField.GetAttribute("value");

            // this could mean that linkedin has removed spaces for us. Clear the field and try again
            if (actualValue.Contains(' ') == false)
            {
                _humanBehaviorService.RandomWaitMilliSeconds(700, 950);
                _logger.LogDebug("Prospect's name that was entered did not contain any spaces. Entered value is {0}", actualValue);
                _humanBehaviorService.DeleteValue(inputField, actualValue, 100, 250);
                return false;
            }

            if (actualValue != prospectName)
            {
                _logger.LogWarning("The prospect name that was entered {0}, did not equal to what the input field received {1}", prospectName, actualValue);
                return false;
            }

            // wait until filtered results are displayed
            _humanBehaviorService.RandomWaitMilliSeconds(1500, 2500);

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
                _connectionsView.ClickProspectSearchInputField(webDriver);
                // use backspace sometimes, or ctrl + a + del

                string enteredValue = inputField.GetAttribute("value");
                if (string.IsNullOrEmpty(enteredValue) == false)
                {
                    _humanBehaviorService.RandomWaitMilliSeconds(950, 1200);
                    _humanBehaviorService.DeleteValue(inputField, enteredValue, 50, 350);
                }

                string valueAfterDelete = inputField.GetAttribute("value");
                if (string.IsNullOrEmpty(valueAfterDelete) == false)
                {
                    _logger.LogError("The search recently added prospects input field has not been successfully cleared");
                    _connectionsView.ClickProspectSearchInputField(webDriver);
                    _humanBehaviorService.RandomWaitMilliSeconds(950, 1200);
                    _humanBehaviorService.DeleteValue(inputField, enteredValue, 50, 350);

                    valueAfterDelete = inputField.GetAttribute("value");
                    succeeded = string.IsNullOrEmpty(valueAfterDelete) != false;
                }
                else
                {
                    succeeded = true;
                }
            }
            catch
            {
                _logger.LogDebug("Failed to clear prospect filter input field");
            }

            return succeeded;
        }

        public bool EnsureRecentlyAddedHitlistRendered(IWebDriver webDriver)
        {
            bool succeeded = false;
            _humanBehaviorService.RandomWaitMilliSeconds(1240, 1950);

            RecentlyAddedResults result = _connectionsView.DetermineRecentlyAddedResults(webDriver);
            if (result != RecentlyAddedResults.HitList)
            {
                // just do a simple refresh
                succeeded = _webDriverProvider.Refresh(webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(1240, 1950);
            }
            else
            {
                succeeded = true;
            }

            return succeeded;
        }

        public void ClickElipses(IWebElement prospect)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(750, 950);

            IWebElement elipsesButton = _connectionsView.GetElipsesButton(prospect);
            _humanBehaviorService.RandomClickElement(elipsesButton);
            _humanBehaviorService.RandomWaitMilliSeconds(1010, 1250);
            int number = _rnd.Next(1, 5);
            if (number == 3)
            {
                _humanBehaviorService.RandomClickElement(elipsesButton);
            }
        }
    }
}
