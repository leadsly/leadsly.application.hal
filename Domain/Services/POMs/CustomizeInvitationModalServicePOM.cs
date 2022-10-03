﻿using Domain.POMs.Dialogs;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Services.POMs
{
    public class CustomizeInvitationModalServicePOM : ICustomizeInvitationModalServicePOM
    {
        public CustomizeInvitationModalServicePOM(ILogger<CustomizeInvitationModalServicePOM> logger, IHumanBehaviorService humanBehaviorService, ICustomizeYourInvitationDialog dialog)
        {
            _logger = logger;
            _dialog = dialog;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ICustomizeYourInvitationDialog _dialog;
        private readonly ILogger<CustomizeInvitationModalServicePOM> _logger;

        public bool HandleInteraction(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement modalContent = _dialog.Content(webDriver);

            _humanBehaviorService.RandomClickElement(modalContent);

            _humanBehaviorService.RandomWaitMilliSeconds(1000, 2000);
            bool clickSucceeded = _dialog.SendConnection(webDriver);
            if (clickSucceeded == false)
            {
                _logger.LogDebug("Clicking 'Send' button on the modal failed");
                succeeded = false;
            }
            else
            {
                _logger.LogDebug("Clicking 'Send' button on the modal succeeded");
                succeeded = true;
            }

            return succeeded;
        }

        public void CloseDialog(IWebDriver webDriver)
        {
            _dialog.CloseDialog(webDriver);
        }
    }
}