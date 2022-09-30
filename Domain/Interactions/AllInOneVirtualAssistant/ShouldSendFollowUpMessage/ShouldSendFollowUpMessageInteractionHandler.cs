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

        public IWebElement PopupConversation { get; private set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            ShouldSendFollowUpMessageInteraction shouldSendInteraction = interaction as ShouldSendFollowUpMessageInteraction;
            IWebDriver webDriver = shouldSendInteraction.WebDriver;
            // a. If this is the first follow up message going out enter in the message and click send
            if (_service.ClickMessageProspect(webDriver, shouldSendInteraction.Prospect) == false)
            {
                _logger.LogError("Could not click on Message prospect");
                return false;
            }

            if (string.IsNullOrEmpty(shouldSendInteraction.PreviousMessageContent) == true)
            {
                _logger.LogInformation("This is the first follow up message that is going out.");
                return true;
            }
            else
            {
                IWebElement popupConversation = _service.GetPopUpConversation(webDriver, shouldSendInteraction.ProspectName);

                if (popupConversation != null)
                {
                    PopupConversation = popupConversation;

                    if (_service.IsThereConversationHistory(popupConversation) == true)
                    {
                        // c. if this is not the first follow up message, grab the last message from the conversation list and check to see if it was sent from the prospect
                        IWebElement lastMessage = _service.Messages.LastOrDefault();
                        if (lastMessage != null)
                        {
                            bool? wasLastMessageSentByProspect = _service.WasLastMessageSentByProspect(lastMessage, shouldSendInteraction.ProspectName);
                            if (wasLastMessageSentByProspect == null)
                            {
                                return false;
                            }
                            else if (_service.WasLastMessageSentByProspect(lastMessage, shouldSendInteraction.ProspectName) == true)
                            {
                                // we've gotten a reply, we need to mark this prospect as complete. Do NOT send anymore follow up messages
                                ProspectRepliedModel prospect = new()
                                {
                                    ResponseMessageTimestamp = _timestampService.TimestampNow(),
                                    CampaignProspectId = shouldSendInteraction.CampaignProspectId,
                                    ResponseMessage = lastMessage.Text,
                                    Name = shouldSendInteraction.ProspectName
                                };

                                Prospect = prospect;
                                DidProspectReply = true;

                                return false;
                            }
                            else
                            {
                                bool? messagesMatch = _service.DoesLastMessageMatchPreviouslySentMessage(lastMessage, shouldSendInteraction.PreviousMessageContent);
                                // check if the contents of the last follow up message match the contents of the last
                                if (messagesMatch == null)
                                {
                                    return false;
                                }
                                else if (messagesMatch == true)
                                {
                                    return true;
                                }
                                else
                                {
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

        public ProspectRepliedModel GetProspect()
        {
            ProspectRepliedModel prospect = Prospect;
            Prospect = null;
            return prospect;
        }
    }
}
