using Domain.POMs.Dialogs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace PageObjects.Dialogs.SearchPageDialogs
{
    public class CustomizeYourInvitationDialog : SendConnectionDialogBase, ICustomizeYourInvitationDialog
    {
        public CustomizeYourInvitationDialog(ILogger<CustomizeYourInvitationDialog> logger, IWebDriverUtilities webDriverUtilities) : base(logger, webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly ILogger<CustomizeYourInvitationDialog> _logger;

        public IWebElement Content(IWebDriver webDriver)
        {
            IWebElement modalContent = default;
            try
            {
                _logger.LogInformation("Locating custimize modal content by class name 'artdeco-modal__content'");
                modalContent = Modal(webDriver)?.FindElement(By.ClassName("artdeco-modal__content"));
                if (modalContent != null)
                {
                    _logger.LogInformation("Found 'Customize Your Invitation' modal content");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate customize modal content.");
            }
            return modalContent;
        }

        public bool SendConnection(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement button = _webDriverUtilities.WaitUntilNotNull(SendButton, webDriver, 5);
            if (button == null)
            {
                succeeded = false;
            }
            else
            {
                _logger.LogInformation("Clicking 'Send' button inside the Customize Your Invitation dialog");
                button.Click();
                succeeded = true;
            }
            return succeeded;
        }

        private IWebElement Modal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                _logger.LogDebug("Locating custimize modal by class name 'artdeco-modal'");
                modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal[role='dialog']"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to locate 'Customize Your Invitation' modal");
            }
            return modal;
        }

        private IWebElement SendButton(IWebDriver webDriver)
        {
            IWebElement button = default;
            try
            {
                _logger.LogInformation("Finding 'Send' button for the given prospect.");
                button = webDriver.FindElement(By.CssSelector("button[aria-label='Send now']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate 'Send' button");
            }
            return button;
        }
    }
}
