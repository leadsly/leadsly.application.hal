using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage;
using Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName;
using Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded;
using Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.InstructionSets
{
    public class FollowUpMessageInstructionSet : IFollowUpMessageInstructionSet
    {
        public FollowUpMessageInstructionSet(
            ILogger<FollowUpMessageInstructionSet> logger,
            IFollowUpMessageInteractionFacade interactionFacade)
        {
            _interactionFacade = interactionFacade;
            _logger = logger;
        }

        private readonly IFollowUpMessageInteractionFacade _interactionFacade;
        private readonly ILogger<FollowUpMessageInstructionSet> _logger;
        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }
        public SentFollowUpMessageModel GetSentFollowUpMessage()
        {
            SentFollowUpMessageModel item = SentFollowUpMessage;
            SentFollowUpMessage = null;
            return item;
        }

        public void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message)
        {
            if (CreateNewMessageInteraction(webDriver) == false)
            {
                return;
            }

            if (EnterProspectNameInteraction(webDriver, message.ProspectName) == false)
            {
                return;
            }

            if (EnterMessageInteraction(webDriver, message.Content, message.OrderNum) == false)
            {
                return;
            }

            SentFollowUpMessage = _interactionFacade.SentFollowUpMessage;
        }

        public void SendFollowUpMessage_AllInOne(IWebDriver webDriver, FollowUpMessageBody message)
        {
            // 1. check if the prospect name is in the list of recently added connections
            if (IsProspectInRecentlyAddedListInteraction(webDriver, message.ProspectName) == true)
            {
                IWebElement propsectFromTheHitlist = _interactionFacade.ProspectFromRecentlyAdded;
                // 2. if it is click message
                if (ShouldSendFollowUpMessageInteraction(webDriver, propsectFromTheHitlist, message) == true)
                {
                    IWebElement popupConversation = _interactionFacade.PopupConversation;
                    if (EnterFollowUpMessageInteraction(webDriver, popupConversation, message) == false)
                    {

                    }

                    // something went wrong, we will attempt to send follow up message next time
                }
                else
                {
                    if (_interactionFacade.DidProspectReply == true)
                    {
                        // we need to mark the prospect as complete, thery should get no more follow up messages
                        // emit an event to update this prospects as replied
                        ProspectRepliedModel prospectReplied = _interactionFacade.ProspectReplied;

                    }
                    else
                    {
                        // something went wrong
                        _logger.LogError("Something went wrong when attempting to send a follow up message");
                    }
                }
            }
            else
            {
                if (EnterProspectsNameIntoSearchByNameFieldInteraction(webDriver, message.ProspectName) == true)
                {
                    IWebElement propsectFromTheHitlist = _interactionFacade.ProspectFromRecentlyAdded;
                    // 2. if it is click message
                    if (ShouldSendFollowUpMessageInteraction(webDriver, propsectFromTheHitlist, message) == true)
                    {
                        IWebElement popupConversation = _interactionFacade.PopupConversation;
                        if (EnterFollowUpMessageInteraction(webDriver, popupConversation, message) == false)
                        {

                        }

                        // something went wrong, we will attempt to send follow up message next time
                    }
                    else
                    {
                        // we need to mark the prospect as complete, thery should get no more follow up messages   
                    }
                }

                // something went wrong, we will attempt to send follow up message next time
            }


            // a. If this is the first follow up message going out enter in the message and click send

            // b. then close the message popup window

            // c. if this is not the first follow up message, grab the last message from the conversation list and check to see if it was sent from the prospect

            // d. if the last message was sent from the prospect it means we have a reply, do not send a follow up message

            // e . if the last message was NOT sent from the prospect, check if it matches the last message we sent

            // f . if it does match the last follow up message we sent, then send the follow up message, else do not send

            // 3. if it is not enter in the prospects name in the search by name input field
        }

        private bool IsProspectInRecentlyAddedListInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new CheckIfProspectIsInRecentlyAddedListInteraction
            {
                WebDriver = webDriver,
                ProspectName = prospectName
            };

            return _interactionFacade.HandleCheckIfProspectExistsInRecentlyAddedInteraction(interaction);
        }

        private bool ShouldSendFollowUpMessageInteraction(IWebDriver webDriver, IWebElement prospectFromTheHitlist, FollowUpMessageBody message)
        {

            InteractionBase interaction = new ShouldSendFollowUpMessageInteraction
            {
                WebDriver = webDriver,
                PreviousMessageContent = message.PreviousMessageContent,
                ProspectName = message.ProspectName,
                Prospect = prospectFromTheHitlist,
                CampaignProspectId = message.CampaignProspectId
            };

            return _interactionFacade.HandleShouldSendFollowUpMessageInteraction(interaction);
        }

        private bool EnterFollowUpMessageInteraction(IWebDriver webDriver, IWebElement popupConversation, FollowUpMessageBody message)
        {
            InteractionBase interaction = new EnterFollowUpMessageInteraction
            {
                WebDriver = webDriver,
                Content = message.Content,
                OrderNum = message.OrderNum,
                PopUpConversation = popupConversation
            };

            return _interactionFacade.HandleEnterFollowUpMessageInteraction(interaction);
        }

        private bool EnterProspectsNameIntoSearchByNameFieldInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new EnterProspectNameIntoSearchInteraction
            {
                WebDriver = webDriver,
                ProspectName = prospectName
            };

            return _interactionFacade.HandleEnterProspectNameIntoSearchByNameFieldInteraction(interaction);
        }

        private bool CreateNewMessageInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new CreateNewMessageInteraction
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleCreateNewMessageInteraction(interaction);
        }

        private bool EnterProspectNameInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new EnterProspectNameInteraction
            {
                ProspectName = prospectName,
                WebDriver = webDriver
            };

            return _interactionFacade.HandleEnterProspectNameInteraction(interaction);
        }

        private bool EnterMessageInteraction(IWebDriver webDriver, string content, int orderNum)
        {
            InteractionBase interaction = new EnterMessageInteraction
            {
                Content = content,
                WebDriver = webDriver,
                OrderNum = orderNum
            };

            return _interactionFacade.HandleEnterMessageInteraction(interaction);
        }
    }
}
