using Domain.POMs.Controls;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ScreenHouseKeeperService : IScreenHouseKeeperService
    {
        public ScreenHouseKeeperService(ILogger<ScreenHouseKeeperService> logger, IConversationCards conversationCards)
        {
            _logger = logger;
            _conversationCards = conversationCards;
        }

        private readonly ILogger<ScreenHouseKeeperService> _logger;
        private readonly IConversationCards _conversationCards;

        public bool CloseConversation(IWebElement closeButton)
        {
            bool closedSuccessfully = false;
            try
            {
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
    }
}
