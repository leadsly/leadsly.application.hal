using Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp
{
    public class PrepareProspectForFollowUpMessageInteractionHandler : IPrepareProspectForFollowUpMessageInteractionHandler
    {
        public PrepareProspectForFollowUpMessageInteractionHandler(
            ILogger<PrepareProspectForFollowUpMessageInteractionHandler> logger,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<PrepareProspectForFollowUpMessageInteractionHandler> _logger;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        private IWebElement _popupConversation;
        public IWebElement PopupConversation
        {
            get
            {
                IWebElement popupConversation = _popupConversation;
                _popupConversation = null;
                return popupConversation;
            }
            private set
            {
                _popupConversation = value;
            }
        }

        public bool HandleInteraction(InteractionBase interaction)
        {
            PrepareProspectForFollowUpMessageInteraction prepareInteraction = interaction as PrepareProspectForFollowUpMessageInteraction;
            IWebDriver webDriver = prepareInteraction.WebDriver;
            IWebElement prospectFromTheHitlist = prepareInteraction.ProspectFromTheHitlist;
            string prospectName = prepareInteraction.ProspectName;

            if (prospectFromTheHitlist == null)
            {
                _logger.LogError("Expected to receive prospect from the recently added hit list, but got null");
                return false;
            }

            if (_service.ClickMessageProspect(webDriver, prospectFromTheHitlist) == false)
            {
                _logger.LogError("Could not click 'Message' on the prospect in the Recently Added list");
                return false;
            }

            // locate the popup message that was just launched and return it
            IWebElement popupConversation = _service.GetPopUpConversation(webDriver, prospectName);
            if (popupConversation == null)
            {
                _logger.LogError("Expected to find the popup conversation by prospect name {0}, but none was found", prospectName);
                return false;
            }

            _logger.LogInformation("Popup conversation was found!");

            PopupConversation = popupConversation;
            return true;
        }
    }
}
