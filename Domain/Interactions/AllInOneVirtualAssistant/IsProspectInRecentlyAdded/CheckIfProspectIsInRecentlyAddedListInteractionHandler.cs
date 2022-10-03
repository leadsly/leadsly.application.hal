using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage;
using Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded.Interfaces;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded
{
    public class CheckIfProspectIsInRecentlyAddedListInteractionHandler : ICheckIfProspectIsInRecentlyAddedListInteractionHandler
    {
        public CheckIfProspectIsInRecentlyAddedListInteractionHandler(
            ILogger<SendFollowUpMessageInteractionHandler> logger,
            IHumanBehaviorService humanBehaviorService,
            IFollowUpMessageOnConnectionsServicePOM service)

        {
            _humanBehaviorService = humanBehaviorService;
            _logger = logger;
            _service = service;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<SendFollowUpMessageInteractionHandler> _logger;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        public IWebElement ProspectFromRecentlyAdded { get; private set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            CheckIfProspectIsInRecentlyAddedListInteraction prospectExistsInteraction = interaction as CheckIfProspectIsInRecentlyAddedListInteraction;
            IWebDriver webDriver = prospectExistsInteraction.WebDriver;

            ProspectFromRecentlyAdded = _service.GetProspectFromRecentlyAdded(webDriver, prospectExistsInteraction.ProspectName, prospectExistsInteraction.ProfileUrl, prospectExistsInteraction.IsFilteredByProspectName);
            if (ProspectFromRecentlyAdded == null)
            {
                return false;
            }

            return true;
        }
    }
}
