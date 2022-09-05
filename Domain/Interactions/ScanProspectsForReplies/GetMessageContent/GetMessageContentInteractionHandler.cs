using Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages;
using Domain.Models.ScanProspectsForReplies;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Interactions.ScanProspectsForReplies.GetMessageContent
{
    public class GetMessageContentInteractionHandler : IGetMessageContentInteractionHandler
    {
        public GetMessageContentInteractionHandler(ILogger<GetNewMessagesInteractionHandler> logger, IScanProspectsServicePOM service, ITimestampService timeStampService)
        {
            _logger = logger;
            _timestampService = timeStampService;
            _service = service;
        }

        private readonly ILogger<GetNewMessagesInteractionHandler> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IScanProspectsServicePOM _service;
        private NewMessage NewMessage { get; set; }
        public bool HandleInteraction(InteractionBase interaction)
        {
            GetMessageContentInteraction getMessageContentInteraction = interaction as GetMessageContentInteraction;

            bool clickSucceeded = _service.ClickNewMessage(getMessageContentInteraction.Message, getMessageContentInteraction.WebDriver);
            if (clickSucceeded == false)
            {
                _logger.LogError("Could not click new message");
                return false;
            }

            IList<IWebElement> messageContents = _service.GetMessageContent(getMessageContentInteraction.WebDriver);
            IWebElement messageContent = messageContents?.LastOrDefault();
            if (messageContent != null)
            {
                _logger.LogDebug("Message content was found");
                NewMessage newMessage = new()
                {
                    ProspectName = _service.ProspectNameFromMessage(getMessageContentInteraction.Message),
                    ResponseMessage = messageContent.Text,
                    ResponseMessageTimestamp = _timestampService.TimestampNow()
                };

                NewMessage = newMessage;
            }

            return true;
        }

        public NewMessage GetNewMessage()
        {
            NewMessage newMessage = NewMessage;
            NewMessage = null;
            return newMessage;
        }
    }
}
