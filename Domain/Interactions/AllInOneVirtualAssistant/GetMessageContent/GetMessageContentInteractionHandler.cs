using Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent.Interfaces;
using Domain.Models.ScanProspectsForReplies;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent
{
    public class GetMessageContentInteractionHandler : IGetMessageContentInteractionHandler
    {
        public GetMessageContentInteractionHandler(
            ILogger<GetMessageContentInteractionHandler> logger,
            IMessageListBubbleServicePOM service,
            ITimestampService timestampService
            )
        {
            _timestampService = timestampService;
            _logger = logger;
            _service = service;
        }

        private readonly ITimestampService _timestampService;
        private readonly ILogger<GetMessageContentInteractionHandler> _logger;
        private readonly IMessageListBubbleServicePOM _service;
        private NewMessageModel NewMessage { get; set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            IWebDriver webDriver = interaction.WebDriver;
            GetMessageContentInteraction getMessageInteraction = interaction as GetMessageContentInteraction;

            if (_service.ClickNewMessage(getMessageInteraction.Message, webDriver) == false)
            {
                _logger.LogError("Could not click new message");
                return false;
            }

            string messageContent = _service.GetMessageContent(getMessageInteraction.Message);
            if (messageContent == null)
            {
                _logger.LogError("Could not get message content");
                return false;
            }

            _logger.LogDebug("Message content was found");
            NewMessageModel newMessage = new()
            {
                ProspectName = _service.ProspectNameFromMessage(getMessageInteraction.Message),
                ResponseMessage = messageContent,
                ResponseMessageTimestamp = _timestampService.TimestampNow()
            };

            NewMessage = newMessage;

            return true;
        }

        public NewMessageModel GetNewMessage()
        {
            NewMessageModel newMessage = NewMessage;
            NewMessage = null;
            return newMessage;
        }
    }
}
