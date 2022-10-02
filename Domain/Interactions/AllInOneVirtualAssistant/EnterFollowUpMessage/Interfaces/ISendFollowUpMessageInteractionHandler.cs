using Domain.Models.FollowUpMessage;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage.Interfaces
{
    public interface ISendFollowUpMessageInteractionHandler : IInteractionHandler
    {
        public SentFollowUpMessageModel GetSentFollowUpMessageModel();
    }
}
