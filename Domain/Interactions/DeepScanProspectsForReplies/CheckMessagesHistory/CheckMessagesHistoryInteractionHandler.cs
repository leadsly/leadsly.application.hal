using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces;
using Domain.Models;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory
{
    public class CheckMessagesHistoryInteractionHandler : ICheckMessagesHistoryInteractionHandler<CheckMessagesHistoryInteraction>
    {
        public CheckMessagesHistoryInteractionHandler(
            ILogger<CheckMessagesHistoryInteractionHandler> logger,
            ITimestampService timestampService,
            IDeepScanProspectsService service)
        {
            _logger = logger;
            _service = service;
            _timestampService = timestampService;
        }

        private readonly ITimestampService _timestampService;
        private readonly IDeepScanProspectsService _service;
        private readonly ILogger<CheckMessagesHistoryInteractionHandler> _logger;

        public ProspectReplied Prospect { get; set; }

        public bool HandleInteraction(CheckMessagesHistoryInteraction interaction)
        {
            _logger.LogDebug("Executing CheckMessagesHistoryInteraction.");
            IList<IWebElement> messageContents = _service.GetMessageContents(interaction.WebDriver);
            IWebElement targetMessage = messageContents.Where(m => m.Text.Contains(interaction.TargetMessage)).FirstOrDefault();
            if (targetMessage == null)
            {
                _logger.LogError("Target message not found");
                return false;
            }

            // this is the message that we have sent to this prospect
            int targetMessageIndex = messageContents.IndexOf(targetMessage);
            // this is the message right after ours, if prospect responded there should be a message from the prospect at this index
            int nextMessageIndex = targetMessageIndex + 1;

            // check if any messages after targetMessageIndex are from the prospect
            for (int i = nextMessageIndex; i < messageContents.Count; i++)
            {
                IWebElement nextMessage = messageContents.ElementAt(i);
                string prospectNameFromMessage = _service.GetProspectNameFromMessageContent(nextMessage);
                _logger.LogDebug("Target prospect name {0}, prospect name found in the messages {1}", interaction.ProspectName, prospectNameFromMessage);
                if (interaction.ProspectName == prospectNameFromMessage)
                {
                    _logger.LogDebug("Prospect {0} responded to our message", interaction.ProspectName);
                    ProspectReplied prospect = new()
                    {
                        ResponseMessageTimestamp = _timestampService.TimestampNow(),
                        CampaignProspectId = interaction.CampaignProspectId,
                        ResponseMessage = nextMessage.Text,
                        Name = interaction.ProspectName
                    };

                    Prospect = prospect;
                    break;
                }
            }

            return true;
        }
    }
}
