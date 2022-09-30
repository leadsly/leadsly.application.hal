using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage
{
    public class EnterFollowUpMessageInteractionHandler : IEnterFollowUpMessageInteractionHandler
    {
        public EnterFollowUpMessageInteractionHandler(
            ILogger<EnterFollowUpMessageInteractionHandler> logger,
            ITimestampService timestampService,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _timestampService = timestampService;
            _service = service;
        }

        private readonly ILogger<EnterFollowUpMessageInteractionHandler> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }
        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterFollowUpMessageInteraction enterFollowUpMessage = interaction as EnterFollowUpMessageInteraction;

            bool succeeded = _service.EnterMessage(enterFollowUpMessage.WebDriver, enterFollowUpMessage.PopUpConversation, enterFollowUpMessage.Content);
            if (succeeded == false)
            {
                // handle failure or retries here
            }
            else
            {
                SentFollowUpMessage = new()
                {
                    MessageOrderNum = enterFollowUpMessage.OrderNum,
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
