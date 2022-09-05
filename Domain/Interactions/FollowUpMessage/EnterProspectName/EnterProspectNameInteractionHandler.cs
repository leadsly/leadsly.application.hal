using Domain.Interactions.FollowUpMessage.EnterProspectName.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.FollowUpMessage.EnterProspectName
{
    public class EnterProspectNameInteractionHandler : IEnterProspectNameInteractionHandler
    {
        public EnterProspectNameInteractionHandler(
            IFollowUpMessageServicePOM service,
            ILogger<EnterProspectNameInteractionHandler> logger
            )
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<EnterProspectNameInteractionHandler> _logger;
        private readonly IFollowUpMessageServicePOM _service;

        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterProspectNameInteraction enterProspectInteraction = interaction as EnterProspectNameInteraction;
            bool succeeded = _service.EnterProspectName(enterProspectInteraction.WebDriver, enterProspectInteraction.ProspectName);
            if (succeeded == false)
            {
                // handle failures here or retries
            }

            return succeeded;
        }
    }
}
