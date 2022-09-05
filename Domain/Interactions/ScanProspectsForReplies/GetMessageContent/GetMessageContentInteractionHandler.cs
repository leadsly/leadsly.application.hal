using Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Leadsly.Application.Model.Requests;
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
        private NewMessageRequest NewMessageRequest { get; set; }
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
                NewMessageRequest request = new()
                {
                    ProspectName = _service.ProspectNameFromMessage(getMessageContentInteraction.Message),
                    ResponseMessage = messageContent.Text,
                    ResponseMessageTimestamp = _timestampService.TimestampNow()
                };

                NewMessageRequest = request;
            }

            return true;
        }

        public NewMessageRequest GetNewMessageRequest()
        {
            NewMessageRequest request = NewMessageRequest;
            NewMessageRequest = null;
            return request;
        }
    }
}
