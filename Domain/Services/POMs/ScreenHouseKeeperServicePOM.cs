using Domain.POMs.Controls;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Services.POMs
{
    public class ScreenHouseKeeperServicePOM : IScreenHouseKeeperServicePOM
    {
        public ScreenHouseKeeperServicePOM(
            ILogger<ScreenHouseKeeperServicePOM> logger,
            IConversationCards conversationCards,
            IHumanBehaviorService humanBehaviorService)
        {
            _logger = logger;
            _conversationCards = conversationCards;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<ScreenHouseKeeperServicePOM> _logger;
        private readonly IConversationCards _conversationCards;

        public bool CloseConversation(IWebElement closeButton)
        {
            bool closedSuccessfully = false;
            try
            {
                _humanBehaviorService.RandomWaitSeconds(1, 3);
                closeButton.Click();
                closedSuccessfully = true;
            }
            catch (Exception ex)
            {
                closedSuccessfully = false;
            }
            return closedSuccessfully;
        }

        public IReadOnlyCollection<IWebElement> GetAllConversationCardsCloseButtons(IWebDriver webDriver)
        {
            return _conversationCards.GetAllConversationCloseButtons(webDriver);
        }

        public bool CloseCurrentlyFocusedConversation(IWebElement currentPopUpConversation)
        {
            bool succeeded = false;
            IWebElement closeButton = _conversationCards.GetCloseConversationButton(currentPopUpConversation);
            if (closeButton != null)
            {
                try
                {
                    closeButton.Click();
                    succeeded = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to click close button on currently active conversation");
                }
            }

            return succeeded;
        }
    }
}
