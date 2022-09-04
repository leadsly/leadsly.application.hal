using Domain.Interactions;

namespace Domain.Facades.Interfaces
{
    public interface IFollowUpMessageInteractionFacade
    {
        bool HandleCreateNewMessageInteraction(InteractionBase interaction);
        bool HandleEnterMessageInteraction(InteractionBase interaction);
        bool HandleEnterProspectNameInteraction(InteractionBase interaction);
    }
}
