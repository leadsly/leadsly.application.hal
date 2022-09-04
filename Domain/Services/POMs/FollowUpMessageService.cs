using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Services.POMs
{
    public class FollowUpMessageService : IFollowUpMessageService
    {
        public FollowUpMessageService(
            ILogger<FollowUpMessageService> logger,
            IHumanBehaviorService humanBehaviorService,
            ILinkedInMessagingPage linkedInMessagingPage)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _linkedInMessagingPage = linkedInMessagingPage;
        }

        private readonly ILogger<FollowUpMessageService> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILinkedInMessagingPage _linkedInMessagingPage;

        public bool ClickCreateNewMessage(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(600, 1200);

            IWebElement messagingHeader = _linkedInMessagingPage.MessagingHeader(webDriver);
            _humanBehaviorService.RandomClickElement(messagingHeader);

            bool succeeded = _linkedInMessagingPage.ClickCreateNewMessage(webDriver);

            _humanBehaviorService.RandomWaitMilliSeconds(600, 1200);

            return succeeded;

        }

        public bool EnterProspectName(IWebDriver webDriver, string prospectName)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(500, 1050);

            IWebElement newMessageNameInputField = _linkedInMessagingPage.NewMessageNameInput(webDriver);
            if (newMessageNameInputField == null)
            {
                return false;
            }

            foreach (char character in prospectName)
            {
                _humanBehaviorService.EnterValue(newMessageNameInputField, character, 150, 300);
            }

            // verify prospct name is entered as expected
            string actualProspectname = newMessageNameInputField.GetAttribute("value");
            if (string.IsNullOrEmpty(actualProspectname))
            {
                _logger.LogDebug("Could not determine entered value. Got null or an empty string from the input field");
                return false;
            }

            if (actualProspectname != prospectName)
            {
                _logger.LogDebug("Entered prospect name does not match the expected. Entered prospect name is {actualProspectname}, expected is {prospectName}", actualProspectname, prospectName);
                return false;
            }

            _logger.LogDebug("Successfully verified that the entered into the input field matches prospects name");

            return true;
        }

        public bool EnterMessage(IWebDriver webDriver, string content)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(300, 500);

            bool clickSucceeded = _linkedInMessagingPage.ClickWriteAMessageBox(webDriver);
            if (clickSucceeded == false)
            {
                return false;
            }

            IWebElement messageInputField = _linkedInMessagingPage.GetWriteAMessagePTag(webDriver);
            if (messageInputField == null)
            {
                return false;
            }

            foreach (char character in content)
            {
                _humanBehaviorService.EnterValue(messageInputField, character, 150, 300);
            }

            bool sendMessageClick = _linkedInMessagingPage.ClickSendMessage(webDriver);
            if (sendMessageClick == false)
            {
                return false;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(400, 750);

            return true;
        }
    }
}
