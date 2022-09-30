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
            throw new System.NotImplementedException();
        }
    }
}
