using Domain.Interactions.AllInOneVirtualAssistant.CleanUpUiState.Interface;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.CleanUpUiState
{
    public class CleanUpFollowUpMessageUiStateInteractionHandler : ICleanUpFollowUpMessageUiStateInteractionHandler
    {
        public CleanUpFollowUpMessageUiStateInteractionHandler(
            ILogger<CleanUpFollowUpMessageUiStateInteractionHandler> logger,
            IScreenHouseKeeperServicePOM screenHouseKeeperServicePOM,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _screenHouseKeeperService = screenHouseKeeperServicePOM;
            _service = service;
        }

        private readonly ILogger<CleanUpFollowUpMessageUiStateInteractionHandler> _logger;
        private readonly IScreenHouseKeeperServicePOM _screenHouseKeeperService;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;

        public bool HandleInteraction(InteractionBase interaction)
        {
            CleanUpFollowUpMessageUiStateInteraction cleanUpInteraction = interaction as CleanUpFollowUpMessageUiStateInteraction;
            IWebElement conversationPopup = cleanUpInteraction.ConversationPopup;
            IWebDriver webDriver = cleanUpInteraction.WebDriver;
            if (conversationPopup != null)
            {
                _screenHouseKeeperService.CloseCurrentlyFocusedConversation(webDriver, conversationPopup);
            }

            if (_service.ClearProspectFilterInputField(webDriver) == false)
            {
                // try one more time
                _service.ClearProspectFilterInputField(webDriver);
            }
            else
            {
                // verify that the full recently added prospect list is rendered
                if (_service.EnsureRecentlyAddedHitlistRendered(webDriver) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
