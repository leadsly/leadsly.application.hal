using Domain.Interactions.Shared.CloseAllConversations.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Interactions.Shared.CloseAllConversations
{
    public class CloseAllConversationsInteractionHandler : ICloseAllConversationsInteractionHandler
    {
        public CloseAllConversationsInteractionHandler(
            ILogger<CloseAllConversationsInteractionHandler> logger,
            IScreenHouseKeeperServicePOM screenService)
        {
            _logger = logger;
            _screenService = screenService;
        }

        private readonly IScreenHouseKeeperServicePOM _screenService;
        private readonly ILogger<CloseAllConversationsInteractionHandler> _logger;

        public bool HandleInteraction(InteractionBase interaction)
        {
            CloseAllConversationsInteraction closeAllConversationsInteraction = interaction as CloseAllConversationsInteraction;
            _logger.LogInformation("Executing CloseAllConversationsInteraction.");
            IList<bool> succeeded = new List<bool>();
            IList<IWebElement> closeButtons = _screenService.GetAllConversationCardsCloseButtons(closeAllConversationsInteraction.WebDriver);
            foreach (IWebElement closeButton in closeButtons)
            {
                succeeded.Add(_screenService.CloseConversation(closeButton));
            }

            return succeeded.All(x => x == true);
        }
    }
}
