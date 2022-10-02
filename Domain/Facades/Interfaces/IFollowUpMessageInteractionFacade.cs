using Domain.Interactions;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using OpenQA.Selenium;

namespace Domain.Facades.Interfaces
{
    public interface IFollowUpMessageInteractionFacade
    {
        public SentFollowUpMessageModel SentFollowUpMessage { get; }
        public SentFollowUpMessageModel SentFollowUpMessage_AllInOne { get; }
        public bool DidProspectReply { get; }
        public IWebElement ProspectFromRecentlyAdded { get; }
        public IWebElement PopupConversation { get; }
        public ProspectRepliedModel ProspectReplied { get; }
        bool HandleCreateNewMessageInteraction(InteractionBase interaction);
        bool HandleEnterMessageInteraction(InteractionBase interaction);
        bool HandleEnterProspectNameInteraction(InteractionBase interaction);
        bool HandleCheckIfProspectExistsInRecentlyAddedInteraction(InteractionBase interaction);
        bool HandleShouldSendFollowUpMessageInteraction(InteractionBase interaction);
        bool HandleSendFollowUpMessageInteraction(InteractionBase interaction);
        bool HandleEnterProspectNameIntoSearchByNameFieldInteraction(InteractionBase interaction);
        bool HandlePrepareProspectForFollowUpMessageInteraction(InteractionBase interaction);
        bool HandleCleanUpFollowUpMessageUiStateInteraction(InteractionBase interaction);
    }
}
