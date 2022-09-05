using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.POMs
{
    public class ScanProspectsServicePOM : IScanProspectsServicePOM
    {
        public ScanProspectsServicePOM(
            ILinkedInMessagingPage linkedInMessagingPage,
            ILogger<ScanProspectsServicePOM> logger,
            IHumanBehaviorService humanBehaviorService)
        {
            _linkedInMessagingPage = linkedInMessagingPage;
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<ScanProspectsServicePOM> _logger;
        private readonly ILinkedInMessagingPage _linkedInMessagingPage;

        public bool ClickNewMessage(IWebElement newMessage, IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(1000, 1500);
            if (_linkedInMessagingPage.ClickConverstaionListItem(newMessage, webDriver) == false)
            {
                return false;
            }
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1300);
            return true;
        }

        public IList<IWebElement> GetMessageContent(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 1200);
            return _linkedInMessagingPage.GetMessageContents(webDriver);
        }

        public IList<IWebElement> GetNewMessages(IWebDriver webDriver)
        {
            IList<IWebElement> newMessages = new List<IWebElement>();
            _humanBehaviorService.RandomWaitMilliSeconds(300, 700);
            IList<IWebElement> visibleConversationListItems = _linkedInMessagingPage.GetVisibleConversationListItems(webDriver);
            if (visibleConversationListItems == null)
            {
                return newMessages;
            }

            newMessages = GrabNewMessagesListItems(visibleConversationListItems, webDriver);
            return newMessages;
        }

        private IList<IWebElement> GrabNewMessagesListItems(IList<IWebElement> listItems, IWebDriver webDriver)
        {
            IList<IWebElement> newConversationListItem = new List<IWebElement>();
            foreach (IWebElement listItem in listItems)
            {
                // if it is first message we are already on there are no notifications, so we need to check if new message label is visible
                if (_linkedInMessagingPage.IsActiveMessageItem(listItem) == true)
                {
                    _logger.LogDebug("This message item is currently selected in the view. Checking for new messages label");
                    if (_linkedInMessagingPage.NewMessageLabel(webDriver) == true)
                    {
                        _logger.LogInformation($"This ListItem [{listItem.Text}] does contain a new notification");
                        newConversationListItem.Add(listItem);
                        continue;
                    }
                }

                if (_linkedInMessagingPage.HasNotification(listItem) == true)
                {
                    _logger.LogInformation($"This ListItem [{listItem.Text}] does contain a new notification");
                    newConversationListItem.Add(listItem);
                }
            }

            return newConversationListItem;
        }

        public string ProspectNameFromMessage(IWebElement element)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(700, 900);
            return _linkedInMessagingPage.GetProspectNameFromConversationItem(element);
        }

        public void WaitAndRelaxSome()
        {
            _logger.LogDebug("Waiting some time before continuing to check for new messages");
            _humanBehaviorService.RandomWaitSeconds(30, 50);
        }
    }
}
