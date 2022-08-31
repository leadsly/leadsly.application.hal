using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.POMs
{
    public class DeepScanProspectsService : IDeepScanProspectsService
    {
        public DeepScanProspectsService(
            ILinkedInMessagingPage linkedInMessagingPage,
            ILogger<DeepScanProspectsService> logger,
            IHumanBehaviorService humanBehaviorService)
        {
            _linkedInMessagingPage = linkedInMessagingPage;
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<DeepScanProspectsService> _logger;
        private readonly ILinkedInMessagingPage _linkedInMessagingPage;

        public int GetVisibleConversationCount(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(500, 700);
            IList<IWebElement> visibleConversationListItems = _linkedInMessagingPage.GetVisibleConversationListItems(webDriver);
            if (visibleConversationListItems == null)
            {
                return 0;
            }

            return visibleConversationListItems.Count;
        }

        public bool ClearMessagingSearchCriteria(IWebDriver webDriver)
        {
            IWebElement messagingHeader = _linkedInMessagingPage.MessagingHeader(webDriver);
            _humanBehaviorService.RandomClickElement(messagingHeader);
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1250);

            bool succeeded = _linkedInMessagingPage.ClearMessagingSearchCriteria(webDriver);
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1100);

            return succeeded;
        }

        public bool EnterSearchMessagesCriteria(IWebDriver webDriver, string searchCriteria)
        {
            IWebElement searchInput = _linkedInMessagingPage.SearchMessagesInputField(webDriver);
            if (searchInput == null)
            {
                return false;
            }
            try
            {
                foreach (char character in searchCriteria)
                {
                    _humanBehaviorService.EnterValue(searchInput, character, 200, 300);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured entering in search criteria");
                return false;
            }

            return VerifySearchTermWasEnteredCorrectly(searchInput, searchCriteria);
        }

        public bool ClickNewMessage(IWebElement messageItem, IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(1000, 1500);
            if (_linkedInMessagingPage.ClickConverstaionListItem(messageItem, webDriver) == false)
            {
                return false;
            }
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1300);
            return true;
        }

        private bool VerifySearchTermWasEnteredCorrectly(IWebElement searchInput, string searchCriteria)
        {
            string enteredValue = searchInput.GetAttribute("value");
            if (enteredValue != searchCriteria)
            {
                _logger.LogDebug("Failed to correctly enter search term {searchCriteria}. The entered value was: {enteredValue}", searchCriteria, enteredValue);
                return false;
            }

            return true;
        }

        public IList<IWebElement> GetMessageContents(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1200);
            return _linkedInMessagingPage.GetMessageContents(webDriver);
        }

        public IWebElement GetProspectsMessageItem(IWebDriver webDriver, string prospectName, int beforeSearchMessagesCount)
        {
            bool succeeded = WaitForSearchResultsToDiffer(webDriver, beforeSearchMessagesCount);
            if (succeeded == false)
            {
                return null;
            }

            if (_linkedInMessagingPage.IsNoMessagesDisplayed(webDriver) == true)
            {
                _logger.LogDebug("No messages were found");
                return null;
            }
            _logger.LogDebug("Messages were found");

            IList<IWebElement> visibleMessages = _linkedInMessagingPage.GetVisibleConversationListItems(webDriver);
            if (visibleMessages == null)
            {
                _logger.LogDebug("No visible messages were found. Cannot proceed. Expected to find at least one message");
                return null;
            }
            if (visibleMessages.Count > 1)
            {
                _logger.LogDebug("More than one message found. This is not expected. Expected exactly one result. The search criteria was: {searchCriteria}", prospectName);
                return null;
            }
            if (visibleMessages.Count == 0)
            {
                _logger.LogDebug("No messages found. This is not expected. Expected at least one result. The search criteria was: {searchCriteria}", prospectName);
                return null;
            }

            IWebElement prospectMessage = visibleMessages.FirstOrDefault();
            string messageContent = prospectMessage.Text;
            if (messageContent == null)
            {
                _logger.LogDebug("No message content found. This is not expected. Expected at least one result. The search criteria was: {searchCriteria}", prospectName);
                return null;
            }

            string prospectNameFromMessage = _linkedInMessagingPage.GetProspectNameFromConversationItem(prospectMessage);
            if (prospectNameFromMessage == string.Empty)
            {
                _logger.LogDebug("Unable to determine prospect name from the message. Cannot process this message");
                return null;
            }

            if (prospectNameFromMessage != prospectName)
            {
                _logger.LogDebug("The prospect name from the message {prospectNameFromMessage} did not match the prospect we were looking for {prospectName}.", prospectNameFromMessage, prospectName);
                return null;
            }
            else
            {
                return prospectMessage;
            }
        }

        // this assumes there are more than one conversation
        private bool WaitForSearchResultsToDiffer(IWebDriver webDriver, int messageCountBefore)
        {
            _logger.LogDebug("Waiting for search results to differ. Messages count before search was {0}", messageCountBefore);
            bool searchResultsDiffer = false;
            try
            {
                WebDriverWait waiter = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                waiter.Until((drv) =>
                {
                    IList<IWebElement> visibleMessages = _linkedInMessagingPage.GetVisibleConversationListItems(webDriver);
                    return messageCountBefore != visibleMessages.Count;
                });
                _logger.LogDebug("Search results differ");
                searchResultsDiffer = true;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogWarning(ex, "Search results from before hal entered in search term and AFTER hal entered search term are the same! Most cases they should be different");
                searchResultsDiffer = false;
            }

            return searchResultsDiffer;
        }

        public string GetProspectNameFromMessageContent(IWebElement messageContent)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(500, 1050);
            return _linkedInMessagingPage.GetProspectNameFromMessageContentPTag(messageContent);
        }
    }
}
