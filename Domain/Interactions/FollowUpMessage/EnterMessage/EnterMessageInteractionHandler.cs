using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.FollowUpMessage.EnterMessage
{
    public class EnterMessageInteractionHandler : IEnterMessageInteractionHandler
    {
        public EnterMessageInteractionHandler(
            IFollowUpMessageService service,
            ILogger<EnterMessageInteractionHandler> logger
            )
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<EnterMessageInteractionHandler> _logger;
        private readonly IFollowUpMessageService _service;

        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterMessageInteraction enterMessage = interaction as EnterMessageInteraction;
            bool succeeded = _service.EnterMessage(enterMessage.WebDriver, enterMessage.Content);
            if (succeeded == false)
            {
                // handle failure or retries here
            }

            return succeeded;
        }
    }
}
