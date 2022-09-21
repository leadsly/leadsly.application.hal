using Domain.Interactions;
using Domain.Models.FollowUpMessage;

namespace Domain.Facades.Interfaces
{
    public interface IFollowUpMessageInteractionFacade
    {
        public SentFollowUpMessageModel SentFollowUpMessage { get; }
        bool HandleCreateNewMessageInteraction(InteractionBase interaction);
        bool HandleEnterMessageInteraction(InteractionBase interaction);
        bool HandleEnterProspectNameInteraction(InteractionBase interaction);
    }
}
