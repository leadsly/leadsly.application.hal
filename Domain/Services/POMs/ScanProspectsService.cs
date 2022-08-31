using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.POMs
{
    public class ScanProspectsService : IScanProspectsService
    {
        public ScanProspectsService(
            ILinkedInMessagingPage linkedInMessagingPage,
            ILogger<ScanProspectsService> logger,
            IHumanBehaviorService humanBehaviorService)
        {
            _linkedInMessagingPage = linkedInMessagingPage;
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<ScanProspectsService> _logger;
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

            newMessages = GrabNewMessagesListItems(visibleConversationListItems);
            return newMessages;
        }

        private IList<IWebElement> GrabNewMessagesListItems(IList<IWebElement> listItems)
        {
            IList<IWebElement> newConversationListItem = new List<IWebElement>();
            foreach (IWebElement listItem in listItems)
            {
                _humanBehaviorService.RandomWaitMilliSeconds(600, 900);
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
