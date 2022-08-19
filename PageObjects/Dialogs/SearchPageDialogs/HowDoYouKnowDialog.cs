using Domain;
using Domain.POMs.Dialogs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Dialogs.SearchPageDialogs
{
    public class HowDoYouKnowDialog : SendConnectionDialogBase, IHowDoYouKnowDialog
    {
        public HowDoYouKnowDialog(ILogger<CustomizeYourInvitationDialog> logger, IWebDriverUtilities webDriverUtilities) : base(logger, webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly ILogger<CustomizeYourInvitationDialog> _logger;

        public bool SendConnection(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement button = _webDriverUtilities.WaitUntilNull(ConnectButton, webDriver, 5);
            if (button == null)
            {
                _logger.LogDebug("Failed to locate 'Connect' button");
                succeeded = false;
            }
            else
            {
                _logger.LogInformation("Clicking 'Connect' button inside the How Do You Know dialog");
                button.Click();
                succeeded = true;
            }
            return succeeded;
        }

        public IList<IWebElement> SelectionChoices(IWebDriver webDriver)
        {
            IList<IWebElement> options = _webDriverUtilities.WaitUntilNotNull(Choices, webDriver, 5);
            return options;
        }

        public bool SelectChoice(HowDoYouKnowChoice choice, IList<IWebElement> choices)
        {
            _logger.LogInformation("Selecting 'Other' option from the 'How do you know [prospect]' modal");
            foreach (IWebElement option in choices)
            {
                try
                {
                    _logger.LogDebug("How do you know prospect? The choice selected is: {0}", Enum.GetName(choice));
                    if (option.GetAttribute("aria-label") == Enum.GetName(choice))
                    {
                        option.Click();
                        return true;
                    }
                }
                catch
                {
                    // do nothing
                }
            }
            _logger.LogDebug("The list of given choices did not contain desired choice which is: {0}", Enum.GetName(choice));
            return false;
        }

        public bool VerifySelection(HowDoYouKnowChoice choice, IList<IWebElement> choices)
        {
            bool choiceSelected = false;
            _logger.LogInformation("Verifying that the 'How do you know [prospect]' modal has the correct selection. Desired choice is {0}", Enum.GetName(choice));

            IWebElement choiceElement = choices.FirstOrDefault(x => x.GetAttribute("aria-label") == Enum.GetName(choice));
            if (choiceElement != null)
            {
                _logger.LogDebug("Verify selection successfully found the desired choice {choice}", Enum.GetName(choice));
                try
                {
                    string selected = choiceElement.GetAttribute("aria-checked");
                    if (selected != null)
                    {
                        _logger.LogDebug("Choice contains the expected 'aria-checked' label. The value of it is {0}", selected);
                        choiceSelected = selected == "true";
                    }
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
            else
            {
                _logger.LogDebug("Verify selection did not find the desired choice {choice}", Enum.GetName(choice));
            }

            return choiceSelected;
        }

        private IWebElement ConnectButton(IWebDriver webDriver)
        {
            IWebElement button = default;
            try
            {
                _logger.LogInformation("Finding 'Connect' button inside the 'How do you know modal'.");
                IWebElement modal = _webDriverUtilities.WaitUntilNotNull(Modal, webDriver, 5);
                if (modal != null)
                {
                    button = modal.FindElement(By.CssSelector("button[aria-label='Connect']"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate 'Connect' button inside the 'How do you know modal'.");
            }
            return button;
        }

        private IList<IWebElement> Choices(IWebDriver webDriver)
        {
            IList<IWebElement> choices = default;
            try
            {
                _logger.LogDebug("Locating all of the available choices for 'How Do You Know' modal");
                choices = webDriver.FindElements(By.CssSelector("div[role='dialog'] .artdeco-pill--choice")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to locate available choices for 'How Do You Know' modal");
            }
            return choices;
        }
    }
}
