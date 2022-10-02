using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage
{
    public class SendFollowUpMessageInteractionHandler : ISendFollowUpMessageInteractionHandler
    {
        public SendFollowUpMessageInteractionHandler(
            ILogger<SendFollowUpMessageInteractionHandler> logger,
            ITimestampService timestampService,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _timestampService = timestampService;
            _service = service;
        }

        private readonly ILogger<SendFollowUpMessageInteractionHandler> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }
        public bool HandleInteraction(InteractionBase interaction)
        {
            SendFollowUpMessageInteraction sendFollowUpMessage = interaction as SendFollowUpMessageInteraction;
            IWebDriver webDriver = sendFollowUpMessage.WebDriver;
            IWebElement popupConversation = sendFollowUpMessage.PopUpConversation;

            bool succeeded = _service.SendMessage(webDriver, popupConversation, sendFollowUpMessage.Content);
            if (succeeded == false)
            {
                // handle failure or retries here
            }
            else
            {
                SentFollowUpMessage = new()
                {
                    MessageOrderNum = sendFollowUpMessage.OrderNum,
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
