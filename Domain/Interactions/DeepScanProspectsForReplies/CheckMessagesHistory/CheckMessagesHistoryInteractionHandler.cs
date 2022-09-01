﻿using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces;
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

        private ProspectReplied Prospect { get; set; }

        public bool HandleInteraction(CheckMessagesHistoryInteraction interaction)
        {
            // click on the new message
            bool clickSucceeded = _service.ClickNewMessage(interaction.MessageListItem, interaction.WebDriver);
            if (clickSucceeded == false)
            {
                _logger.LogDebug("Failed to click on the new message");
                return false;
            }

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
            for (int i = targetMessageIndex; i < messageContents.Count; i++)
            {
                IWebElement nextMessage = messageContents.ElementAt(i);
                string nameFromMessage = _service.GetProspectNameFromMessageContent(nextMessage);
                _logger.LogDebug("Target prospect name {0}, name found in the messages {1}", interaction.ProspectName, nameFromMessage);

                // this means the message was not from us
                if (interaction.LeadslyUserFullName != nameFromMessage)
                {
                    if (interaction.ProspectName == nameFromMessage)
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
                    else
                    {
                        _logger.LogDebug("This message was not sent by leadsly user, however the name found in the message did not match prospects");
                    }
                }
                else
                {
                    _logger.LogDebug("This message was sent by the user ignore it");
                }
            }

            return true;
        }

        public ProspectReplied GetProspect()
        {
            ProspectReplied prospect = Prospect;
            Prospect = null;
            return prospect;
        }
    }
}
