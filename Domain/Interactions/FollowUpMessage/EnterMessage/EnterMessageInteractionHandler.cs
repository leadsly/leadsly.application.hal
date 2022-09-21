using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.FollowUpMessage.EnterMessage
{
    public class EnterMessageInteractionHandler : IEnterMessageInteractionHandler
    {
        public EnterMessageInteractionHandler(
            IFollowUpMessageServicePOM service,
            ITimestampService timestampService,
            ILogger<EnterMessageInteractionHandler> logger
            )
        {
            _timestampService = timestampService;
            _logger = logger;
            _service = service;
        }

        private readonly ITimestampService _timestampService;
        private readonly ILogger<EnterMessageInteractionHandler> _logger;
        private readonly IFollowUpMessageServicePOM _service;

        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterMessageInteraction enterMessage = interaction as EnterMessageInteraction;
            bool succeeded = _service.EnterMessage(enterMessage.WebDriver, enterMessage.Content);
            if (succeeded == false)
            {
                // handle failure or retries here
            }
            else
            {
                SentFollowUpMessage = new()
                {
                    MessageOrderNum = enterMessage.OrderNum,
                    ActualDeliveryDateTimeStamp = _timestampService.TimestampNow()
                };
            }

            return succeeded;
        }

        public SentFollowUpMessageModel GetSentFollowUpMessageModel()
        {
            SentFollowUpMessageModel sentFollowUpMessageModel = SentFollowUpMessage;
            SentFollowUpMessage = null;
            return sentFollowUpMessageModel;
        }
    }
}
