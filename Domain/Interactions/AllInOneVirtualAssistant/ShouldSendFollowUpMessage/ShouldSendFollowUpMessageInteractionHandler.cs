using Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Linq;

namespace Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage
{
    public class ShouldSendFollowUpMessageInteractionHandler : IShouldSendFollowUpMessageInteractionHandler
    {
        public ShouldSendFollowUpMessageInteractionHandler(
            ILogger<ShouldSendFollowUpMessageInteractionHandler> logger,
            ITimestampService timestampService,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _timestampService = timestampService;
            _service = service;
        }

        private readonly ILogger<ShouldSendFollowUpMessageInteractionHandler> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;

        private const string DefaultLastReplyMessage = "<LinkedIn user most likely sent ad hoc message between follow ups. We consider it as user took follow up into its own hands. No more follow ups were sent>";
        private ProspectRepliedModel Prospect { get; set; }
        private bool _prospectReplied;
        public bool DidProspectReply
        {
            get
            {
                bool temp = _prospectReplied;
                _prospectReplied = false;
                return temp;
            }
            private set
            {
                _prospectReplied = value;
            }
        }

        public bool HandleInteraction(InteractionBase interaction)
        {
            ShouldSendFollowUpMessageInteraction shouldSendInteraction = interaction as ShouldSendFollowUpMessageInteraction;
            IWebDriver webDriver = shouldSendInteraction.WebDriver;
            if (string.IsNullOrEmpty(shouldSendInteraction.PreviousMessageContent) == true)
            {
                _logger.LogInformation("This is the first follow up message that is going out.");
                return true;
            }
            else
            {
                IWebElement conversationPopup = shouldSendInteraction.ConversationPopup;

                if (conversationPopup != null)
                {
                    if (_service.IsThereConversationHistory(conversationPopup) == true)
                    {
                        // c. since this is not the first follow up message, grab the last message from the conversation list and check to see if it was sent from the prospect
                        IWebElement lastMessage = _service.Messages.LastOrDefault();
                        if (lastMessage != null)
                        {
                            bool? wasLastMessageSentByProspect = _service.WasLastMessageSentByProspect(lastMessage, shouldSendInteraction.ProspectName);
                            if (wasLastMessageSentByProspect == null)
                            {
                                return false;
                            }
                            else if (wasLastMessageSentByProspect == true)
                            {
                                TreatProspectAsComplete(shouldSendInteraction, lastMessage.Text);
                                return false;
                            }
                            else
                            {
                                bool? messagesMatch = _service.DoesLastMessageMatchPreviouslySentMessage(lastMessage, shouldSendInteraction.PreviousMessageContent);
                                // check if the contents of the last follow up message match the contents of the last
                                if (messagesMatch == null)
                                {
                                    _logger.LogWarning("Could not determine if last message in the conversation history matched the previously sent message");
                                    return false;
                                }
                                else if (messagesMatch == true)
                                {
                                    _logger.LogDebug("Last message in the conversation history matched last sent message, proceeding with sending the next follow up message");
                                    return true;
                                }
                                else
                                {
                                    _logger.LogWarning("Last message in the conversation history did not match last sent by hal message and it was not sent by the prospect. It mustve been sent by a human. We will not send any more follow up messages");
                                    TreatProspectAsComplete(shouldSendInteraction, string.Empty);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogError("There is conversation history detected, but there was an error getting last conversation");
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        private void TreatProspectAsComplete(ShouldSendFollowUpMessageInteraction shouldSendInteraction, string responseMessage)
        {
            // we've gotten a reply, we need to mark this prospect as complete. Do NOT send anymore follow up messages
            ProspectRepliedModel prospect = new()
            {
                ResponseMessageTimestamp = _timestampService.TimestampNow(),
                CampaignProspectId = shouldSendInteraction.CampaignProspectId,
                ResponseMessage = string.IsNullOrEmpty(responseMessage) ? responseMessage : DefaultLastReplyMessage,
                Name = shouldSendInteraction.ProspectName
            };

            Prospect = prospect;
            DidProspectReply = true;
        }

        public ProspectRepliedModel GetProspect()
        {
            ProspectRepliedModel prospect = Prospect;
            Prospect = null;
            return prospect;
        }
    }
}
