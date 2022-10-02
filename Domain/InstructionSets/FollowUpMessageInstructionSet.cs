using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.AllInOneVirtualAssistant.CleanUpUiState;
using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage;
using Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName;
using Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded;
using Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp;
using Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

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

        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;

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
            if (IsProspectInRecentlyAddedListInteraction(webDriver, message) == true)
            {
                IWebElement prospectFromHitlist = _interactionFacade.ProspectFromRecentlyAdded;
                // prepare prospect for followup message
                if (PrepareProspectForFollowUpMessageInteraction(webDriver, prospectFromHitlist, message) == true)
                {
                    IWebElement popupConversation = _interactionFacade.PopupConversation;
                    if (ShouldSendFollowUpMessageInteraction(webDriver, popupConversation, message) == true)
                    {
                        if (SendFollowUpMessageInteraction(webDriver, popupConversation, message) == false)
                        {
                            _logger.LogDebug("Successfully sent follow up message to {0}", message.ProspectName);
                            SentFollowUpMessage = _interactionFacade.SentFollowUpMessage_AllInOne;
                        }

                        CleanUpFollowUpMessageUiStateInteraction(webDriver, popupConversation);
                        return;
                    }
                    else
                    {
                        if (_interactionFacade.DidProspectReply == true)
                        {
                            // we need to mark the prospect as complete, thery should get no more follow up messages
                            // emit an event to update this prospects as replied
                            OutputProspectThatReplied(message);
                        }
                        else
                        {
                            // something went wrong
                            _logger.LogError("Something went wrong when attempting to send a follow up message");
                        }

                        CleanUpFollowUpMessageUiStateInteraction(webDriver, popupConversation);
                        return;
                    }
                }

                CleanUpFollowUpMessageUiStateInteraction(webDriver);
            }
            else
            {
                if (EnterProspectsNameIntoSearchByNameFieldInteraction(webDriver, message) == true)
                {
                    if (IsProspectInRecentlyAddedListInteraction(webDriver, message, true) == true)
                    {
                        IWebElement prospectFromHitlist = _interactionFacade.ProspectFromRecentlyAdded;
                        // prepare prospect for followup message
                        if (PrepareProspectForFollowUpMessageInteraction(webDriver, prospectFromHitlist, message) == true)
                        {
                            IWebElement popupConversation = _interactionFacade.PopupConversation;
                            if (ShouldSendFollowUpMessageInteraction(webDriver, popupConversation, message) == true)
                            {
                                if (SendFollowUpMessageInteraction(webDriver, popupConversation, message) == false)
                                {
                                    _logger.LogDebug("Successfully sent follow up message to {0}", message.ProspectName);
                                    SentFollowUpMessage = _interactionFacade.SentFollowUpMessage_AllInOne;
                                }

                                CleanUpFollowUpMessageUiStateInteraction(webDriver, popupConversation);
                                return;
                            }
                            else
                            {
                                if (_interactionFacade.DidProspectReply == true)
                                {
                                    // we need to mark the prospect as complete, thery should get no more follow up messages
                                    // emit an event to update this prospects as replied
                                    OutputProspectThatReplied(message);
                                }
                                else
                                {
                                    // something went wrong
                                    _logger.LogError("Something went wrong when attempting to send a follow up message");
                                }

                                CleanUpFollowUpMessageUiStateInteraction(webDriver, popupConversation);
                                return;
                            }
                        }
                        else
                        {
                            CleanUpFollowUpMessageUiStateInteraction(webDriver);
                            return;
                        }
                    }
                }

                CleanUpFollowUpMessageUiStateInteraction(webDriver);
            }
        }

        private void OutputProspectThatReplied(FollowUpMessageBody message)
        {
            ProspectRepliedModel prospectReplied = _interactionFacade.ProspectReplied;
            if (prospectReplied != null)
            {
                _logger.LogInformation("Prospect {0} has replied or we need to mark this prospect as complete.", message.ProspectName);
                ProspectsThatRepliedDetected.Invoke(this, new ProspectsThatRepliedEventArgs(message, new List<ProspectRepliedModel>() { prospectReplied }));
            }
        }

        private bool PrepareProspectForFollowUpMessageInteraction(IWebDriver webDriver, IWebElement prospectFromTheHitlist, FollowUpMessageBody message)
        {
            InteractionBase interaction = new PrepareProspectForFollowUpMessageInteraction
            {
                WebDriver = webDriver,
                ProspectFromTheHitlist = prospectFromTheHitlist,
                ProspectName = message.ProspectName
            };

            return _interactionFacade.HandlePrepareProspectForFollowUpMessageInteraction(interaction);
        }

        private bool CleanUpFollowUpMessageUiStateInteraction(IWebDriver webDriver, IWebElement conversationPopup = null)
        {
            InteractionBase interaction = new CleanUpFollowUpMessageUiStateInteraction
            {
                WebDriver = webDriver,
                ConversationPopup = conversationPopup
            };

            return _interactionFacade.HandleCleanUpFollowUpMessageUiStateInteraction(interaction);
        }

        private bool IsProspectInRecentlyAddedListInteraction(IWebDriver webDriver, FollowUpMessageBody message, bool isFilteredByProspectName = false)
        {
            InteractionBase interaction = new CheckIfProspectIsInRecentlyAddedListInteraction
            {
                WebDriver = webDriver,
                ProspectName = message.ProspectName,
                ProfileUrl = message.ProspectProfileUrl,
                IsFilteredByProspectName = isFilteredByProspectName
            };

            return _interactionFacade.HandleCheckIfProspectExistsInRecentlyAddedInteraction(interaction);
        }

        private bool ShouldSendFollowUpMessageInteraction(IWebDriver webDriver, IWebElement conversationPopup, FollowUpMessageBody message)
        {

            InteractionBase interaction = new ShouldSendFollowUpMessageInteraction
            {
                WebDriver = webDriver,
                PreviousMessageContent = message.PreviousMessageContent,
                ProspectName = message.ProspectName,
                ConversationPopup = conversationPopup,
                CampaignProspectId = message.CampaignProspectId
            };

            return _interactionFacade.HandleShouldSendFollowUpMessageInteraction(interaction);
        }

        private bool SendFollowUpMessageInteraction(IWebDriver webDriver, IWebElement popupConversation, FollowUpMessageBody message)
        {
            InteractionBase interaction = new SendFollowUpMessageInteraction
            {
                WebDriver = webDriver,
                Content = message.Content,
                OrderNum = message.OrderNum,
                PopUpConversation = popupConversation
            };

            return _interactionFacade.HandleSendFollowUpMessageInteraction(interaction);
        }

        private bool EnterProspectsNameIntoSearchByNameFieldInteraction(IWebDriver webDriver, FollowUpMessageBody message)
        {
            InteractionBase interaction = new EnterProspectNameIntoSearchInteraction
            {
                WebDriver = webDriver,
                ProspectName = message.ProspectName
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
