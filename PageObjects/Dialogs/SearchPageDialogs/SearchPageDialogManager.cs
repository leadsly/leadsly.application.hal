using Domain;
using Domain.POMs.Dialogs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace PageObjects.Dialogs.SearchPageDialogs
{
    public class SearchPageDialogManager : ISearchPageDialogManager
    {
        public SearchPageDialogManager(ILogger<SearchPageDialogManager> logger, IWebDriverUtilities webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly ILogger<SearchPageDialogManager> _logger;

        public bool IsConnectWithProspectModalOpen(IWebDriver webDriver)
        {
            IWebElement modal = _webDriverUtilities.WaitUntilNotNull(Modal, webDriver, 5);
            return modal != null;
        }

        public SendInviteModalType DetermineSendInviteModalType(IWebDriver webDriver)
        {
            _logger.LogInformation("Determining Send Invite modal type. This is used to determine the type of modal user is presented with");
            SendInviteModalType result = SendInviteModalType.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = CheckModalType(drv);
                    return result != SendInviteModalType.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        public SendInviteModalType CheckModalType(IWebDriver webDriver)
        {
            IWebElement customizeModal = _webDriverUtilities.WaitUntilNotNull(CustomizeYourInviteModal, webDriver, 3);
            if (customizeModal != null && customizeModal.Displayed)
            {
                _logger.LogDebug("CustomizeInvite modal found");
                return SendInviteModalType.CustomizeInvite;
            }

            IWebElement howDoYouKnowModal = _webDriverUtilities.WaitUntilNotNull(HowDoYouKnowModal, webDriver, 3);
            if (howDoYouKnowModal != null && howDoYouKnowModal.Displayed)
            {
                _logger.LogDebug("HowDoYouKnow modal found");
                return SendInviteModalType.HowDoYouKnow;
            }

            return SendInviteModalType.Unknown;
        }

        private IWebElement HowDoYouKnowModal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                _logger.LogInformation("Locating 'How do you know' modal by CSS selector '#artdeco-modal-outlet .artdeco-modal[role='dialog']'");
                IWebElement header = webDriver.FindElement(By.XPath("//div[@role='dialog']//h2"));
                if (header != null)
                {
                    _logger.LogDebug("Determining if this modal contains 'How do you know' text in it");
                    if (header.Text.Contains("How do you know"))
                    {
                        _logger.LogDebug("This modal contains 'How do you know' in the header");
                        modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal[role='dialog']"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate modal by css selector '#artdeco-modal-outlet .artdeco-modal'. Cannot connect with the prospect");
            }
            return modal;
        }

        private IWebElement CustomizeYourInviteModal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                _logger.LogInformation("Locating customize this invitation modal by CSS selector '#artdeco-modal-outlet .artdeco-modal[role='dialog']'");
                IWebElement header = webDriver.FindElement(By.XPath("//div[@role='dialog']//h2"));
                if (header != null)
                {
                    _logger.LogDebug("Determining if this modal contains 'You can customize' text in it");
                    if (header.Text.Contains("You can customize"))
                    {
                        _logger.LogDebug("This modal contains 'You can custmize' in the header");
                        modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal[role='dialog']"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate modal by css selector '#artdeco-modal-outlet .artdeco-modal[role='dialog']'. Cannot connect with the prospect");
            }
            return modal;
        }

        private IWebElement Modal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal[role='dialog']"));
                _logger.LogDebug("Modal has been located!");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("No modal has been located!");
            }
            return modal;
        }
    }
}
