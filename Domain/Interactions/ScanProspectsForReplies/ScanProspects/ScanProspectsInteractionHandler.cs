using Domain.Interactions.ScanProspectsForReplies.ScanProspects.Interfaces;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Interactions.ScanProspectsForReplies.ScanProspects
{
    public class ScanProspectsInteractionHandler : IScanProspectsInteractionHandler<ScanProspectsInteraction>
    {
        public ScanProspectsInteractionHandler(
            ILogger<ScanProspectsInteractionHandler> logger,
            ITimestampService timestampService,
            IScanProspectsService scanProspectsService)
        {
            _timestampService = timestampService;
            _logger = logger;
            _scanProspectsService = scanProspectsService;
        }

        private readonly ITimestampService _timestampService;
        private readonly IScanProspectsService _scanProspectsService;
        private readonly ILogger<ScanProspectsInteractionHandler> _logger;
        private IList<NewMessageRequest> NewMessageRequests { get; set; } = new List<NewMessageRequest>();

        public bool HandleInteraction(ScanProspectsInteraction interaction)
        {
            _scanProspectsService.WaitAndRelaxSome();

            _logger.LogInformation("Scanning prospects for replies interaction.");
            IWebDriver webDriver = interaction.WebDriver;
            IList<IWebElement> newMessages = _scanProspectsService.GetNewMessages(webDriver);
            foreach (IWebElement newMessage in newMessages)
            {
                bool clickSucceeded = _scanProspectsService.ClickNewMessage(newMessage, webDriver);
                if (clickSucceeded == false)
                {
                    _logger.LogError("Could not click new message");
                    continue;
                }

                IList<IWebElement> messageContents = _scanProspectsService.GetMessageContent(webDriver);
                IWebElement messageContent = messageContents?.LastOrDefault();
                if (messageContent != null)
                {
                    _logger.LogDebug("Message content was found");
                    NewMessageRequest request = new()
                    {
                        ProspectName = _scanProspectsService.ProspectNameFromMessage(newMessage),
                        ResponseMessage = messageContent.Text,
                        ResponseMessageTimestamp = _timestampService.TimestampNow()
                    };

                    NewMessageRequests.Add(request);
                }
            }

            return true;
        }

        public IList<NewMessageRequest> GetNewMessageRequests()
        {
            IList<NewMessageRequest> requests = NewMessageRequests;
            NewMessageRequests = new List<NewMessageRequest>();
            return requests;
        }
    }
}
