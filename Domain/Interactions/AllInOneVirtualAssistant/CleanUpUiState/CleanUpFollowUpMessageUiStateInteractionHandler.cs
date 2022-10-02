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
                _screenHouseKeeperService.CloseCurrentlyFocusedConversation(conversationPopup);
            }

            _service.ClearProspectFilterInputField(webDriver);

            // check if recently added results are back if not refresh


            return true;
        }
    }
}
