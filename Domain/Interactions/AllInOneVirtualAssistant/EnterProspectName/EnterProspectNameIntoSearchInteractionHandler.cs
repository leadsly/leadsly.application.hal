using Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName
{
    public class EnterProspectNameIntoSearchInteractionHandler : IEnterProspectNameIntoSearchInteractionHandler
    {
        public EnterProspectNameIntoSearchInteractionHandler(
            ILogger<EnterProspectNameIntoSearchInteractionHandler> logger,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<EnterProspectNameIntoSearchInteractionHandler> _logger;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterProspectNameIntoSearchInteraction enterProspectInteraction = interaction as EnterProspectNameIntoSearchInteraction;
            if (_service.EnterProspectName(enterProspectInteraction.WebDriver, enterProspectInteraction.ProspectName) == false)
            {
                // handle any failures if desired 


                return false;
            }

            return true;
        }
    }
}
