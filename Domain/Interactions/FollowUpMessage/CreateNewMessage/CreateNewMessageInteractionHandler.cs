using Domain.Interactions.FollowUpMessage.CreateNewMessage.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.FollowUpMessage.CreateNewMessage
{
    public class CreateNewMessageInteractionHandler : ICreateNewMessageInteractionHandler
    {
        public CreateNewMessageInteractionHandler(
            IFollowUpMessageService service,
            ILogger<CreateNewMessageInteractionHandler> logger
            )
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<CreateNewMessageInteractionHandler> _logger;
        private readonly IFollowUpMessageService _service;

        public bool HandleInteraction(InteractionBase interaction)
        {
            CreateNewMessageInteraction createInteraction = interaction as CreateNewMessageInteraction;
            bool succeeded = _service.ClickCreateNewMessage(createInteraction.WebDriver);
            if (succeeded == false)
            {
                _logger.LogDebug("Failed to successfully click 'Create New Message'");
                // handle retries here if we need to
            }

            return succeeded;
        }
    }
}
