using Domain.Models.FollowUpMessage;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage.Interfaces
{
    public interface IEnterFollowUpMessageInteractionHandler : IInteractionHandler
    {
        public SentFollowUpMessageModel GetSentFollowUpMessageModel();
    }
}
