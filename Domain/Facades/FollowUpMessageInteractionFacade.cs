using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.FollowUpMessage.CreateNewMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterProspectName.Interfaces;

namespace Domain.Facades
{
    public class FollowUpMessageInteractionFacade : IFollowUpMessageInteractionFacade
    {
        public FollowUpMessageInteractionFacade(
            ICreateNewMessageInteractionHandler createNewMessageHandler,
            IEnterMessageInteractionHandler enterMessageHandler,
            IEnterProspectNameInteractionHandler enterProspectNameHandler)
        {
            _createNewMessageHandler = createNewMessageHandler;
            _enterMessageHandler = enterMessageHandler;
            _enterProspectNameHandler = enterProspectNameHandler;
        }

        private readonly ICreateNewMessageInteractionHandler _createNewMessageHandler;
        private readonly IEnterMessageInteractionHandler _enterMessageHandler;
        private readonly IEnterProspectNameInteractionHandler _enterProspectNameHandler;

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
    }
}
