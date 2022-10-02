using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.AllInOneVirtualAssistant.CleanUpUiState.Interface;
using Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.CreateNewMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterProspectName.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using OpenQA.Selenium;

namespace Domain.Facades
{
    public class FollowUpMessageInteractionFacade : IFollowUpMessageInteractionFacade
    {
        public FollowUpMessageInteractionFacade(
            ICreateNewMessageInteractionHandler createNewMessageHandler,
            IEnterMessageInteractionHandler enterMessageHandler,
            IEnterProspectNameInteractionHandler enterProspectNameHandler,
            ISendFollowUpMessageInteractionHandler sendFollowUpMessageHandler,
            ICleanUpFollowUpMessageUiStateInteractionHandler cleanUpFollowUpMessageUiStateHandler,
            IEnterProspectNameIntoSearchInteractionHandler enterProspectNameIntoSearchHandler,
            ICheckIfProspectIsInRecentlyAddedListInteractionHandler checkIfProspectIsInRecentlyAddedHandler,
            IShouldSendFollowUpMessageInteractionHandler shouldSendFollowUpMessageHandler,
            IPrepareProspectForFollowUpMessageInteractionHandler prepareProspectForFollowUpMessageInteractionHandler
            )
        {
            _prepareProspectForFollowUpMessageInteractionHandler = prepareProspectForFollowUpMessageInteractionHandler;
            _createNewMessageHandler = createNewMessageHandler;
            _enterMessageHandler = enterMessageHandler;
            _enterProspectNameHandler = enterProspectNameHandler;
            _sendFollowUpMessageHandler = sendFollowUpMessageHandler;
            _enterProspectNameIntoSearchHandler = enterProspectNameIntoSearchHandler;
            _checkIfProspectIsInRecentlyAddedHandler = checkIfProspectIsInRecentlyAddedHandler;
            _shouldSendFollowUpMessageHandler = shouldSendFollowUpMessageHandler;
            _cleanUpFollowUpMessageUiStateHandler = cleanUpFollowUpMessageUiStateHandler;
        }

        private readonly IPrepareProspectForFollowUpMessageInteractionHandler _prepareProspectForFollowUpMessageInteractionHandler;
        private readonly ICreateNewMessageInteractionHandler _createNewMessageHandler;
        private readonly IEnterMessageInteractionHandler _enterMessageHandler;
        private readonly IEnterProspectNameInteractionHandler _enterProspectNameHandler;
        private readonly ISendFollowUpMessageInteractionHandler _sendFollowUpMessageHandler;
        private readonly IEnterProspectNameIntoSearchInteractionHandler _enterProspectNameIntoSearchHandler;
        private readonly ICheckIfProspectIsInRecentlyAddedListInteractionHandler _checkIfProspectIsInRecentlyAddedHandler;
        private readonly IShouldSendFollowUpMessageInteractionHandler _shouldSendFollowUpMessageHandler;
        private readonly ICleanUpFollowUpMessageUiStateInteractionHandler _cleanUpFollowUpMessageUiStateHandler;

        public SentFollowUpMessageModel SentFollowUpMessage => _enterMessageHandler.GetSentFollowUpMessageModel();
        public SentFollowUpMessageModel SentFollowUpMessage_AllInOne => _sendFollowUpMessageHandler.GetSentFollowUpMessageModel();
        public bool DidProspectReply => _shouldSendFollowUpMessageHandler.DidProspectReply;

        public IWebElement PopupConversation => _prepareProspectForFollowUpMessageInteractionHandler.PopupConversation;

        public ProspectRepliedModel ProspectReplied => _shouldSendFollowUpMessageHandler.GetProspect();

        public IWebElement ProspectFromRecentlyAdded => _checkIfProspectIsInRecentlyAddedHandler.ProspectFromRecentlyAdded;

        public bool HandleCreateNewMessageInteraction(InteractionBase interaction)
        {
            return _createNewMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterMessageInteraction(InteractionBase interaction)
        {
            return _enterMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterProspectNameInteraction(InteractionBase interaction)
        {
            return _enterProspectNameHandler.HandleInteraction(interaction);
        }

        public bool HandleCheckIfProspectExistsInRecentlyAddedInteraction(InteractionBase interaction)
        {
            return _checkIfProspectIsInRecentlyAddedHandler.HandleInteraction(interaction);
        }

        public bool HandleShouldSendFollowUpMessageInteraction(InteractionBase interaction)
        {
            return _shouldSendFollowUpMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleSendFollowUpMessageInteraction(InteractionBase interaction)
        {
            return _sendFollowUpMessageHandler.HandleInteraction(interaction);
        }

        public bool HandleEnterProspectNameIntoSearchByNameFieldInteraction(InteractionBase interaction)
        {
            return _enterProspectNameIntoSearchHandler.HandleInteraction(interaction);
        }

        public bool HandlePrepareProspectForFollowUpMessageInteraction(InteractionBase interaction)
        {
            return _prepareProspectForFollowUpMessageInteractionHandler.HandleInteraction(interaction);
        }

        public bool HandleCleanUpFollowUpMessageUiStateInteraction(InteractionBase interaction)
        {
            return _cleanUpFollowUpMessageUiStateHandler.HandleInteraction(interaction);
        }
    }
}
