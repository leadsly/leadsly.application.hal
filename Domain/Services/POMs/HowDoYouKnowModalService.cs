using Domain.POMs.Dialogs;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Services.POMs
{
    public class HowDoYouKnowModalService : IHowDoYouKnowModalService
    {
        public HowDoYouKnowModalService(ILogger<HowDoYouKnowModalService> logger, IHumanBehaviorService humanBehaviorService, IHowDoYouKnowDialog dialog)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _dialog = dialog;
        }

        private readonly IHowDoYouKnowDialog _dialog;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<HowDoYouKnowModalService> _logger;

        public bool HandleInteraction(IWebDriver webDriver)
        {
            _humanBehaviorService.RandomWaitMilliSeconds(1000, 2000);

            IList<IWebElement> choices = _dialog.SelectionChoices(webDriver);

            if (choices == null || choices.Count == 0)
            {
                _logger.LogDebug("No choices were found in the 'How Do You Know' modal.");
                return false;
            }

            _logger.LogDebug($"Found {choices.Count} choices in the 'How Do You Know' modal.");

            _logger.LogDebug("'Other' choice was found in the list of choices.");
            bool optionClicked = _dialog.SelectChoice(HowDoYouKnowChoice.Other, choices);
            if (optionClicked == false)
            {
                _logger.LogDebug($"Clicking '{Enum.GetName(HowDoYouKnowChoice.Other)}' choice button on the modal failed");
                return false;
            }
            else
            {
                _logger.LogDebug("Clicking 'Connect' button on the modal succeeded");
                bool verifyOptionIsSelected = _dialog.VerifySelection(HowDoYouKnowChoice.Other, choices);
                if (verifyOptionIsSelected == true)
                {
                    _humanBehaviorService.RandomWaitMilliSeconds(1000, 2000);
                    return _dialog.SendConnection(webDriver);
                }

                _logger.LogDebug("Verifying that the 'Other' choice is selected failed");
                return false;
            }
        }

        public void CloseDialog(IWebDriver webDriver)
        {
            _dialog.CloseDialog(webDriver);
        }
    }
}
