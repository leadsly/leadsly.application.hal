using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace PageObjects.Dialogs.SearchPageDialogs
{
    public abstract class SendConnectionDialogBase
    {
        public SendConnectionDialogBase(ILogger logger, IWebDriverUtilities webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly ILogger _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;

        public virtual IWebElement Modal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                _logger.LogDebug("Locating custimize modal by class name 'artdeco-modal'");
                modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal[role='dialog']"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to locate modal");
            }
            return modal;
        }

        public virtual void CloseDialog(IWebDriver webDriver)
        {
            _logger.LogInformation("Attemping to close open modal");
            IWebElement closeModalbutton = _webDriverUtilities.WaitUntilNotNull(CloseModalButton, webDriver, 2);
            if (closeModalbutton != null)
            {
                closeModalbutton.Click();
            }
        }

        private IWebElement CloseModalButton(IWebDriver webDriver)
        {
            IWebElement closeButton = default;
            try
            {
                _logger.LogInformation("Locating modal close button");
                closeButton = Modal(webDriver).FindElement(By.CssSelector("button[data-test-modal-close-btn]"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("No close modal was found. This could be because modal is not open.");
            }
            return closeButton;
        }

    }
}
