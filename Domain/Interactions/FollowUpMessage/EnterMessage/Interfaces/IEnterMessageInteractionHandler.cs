using Domain.Models.FollowUpMessage;

namespace Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces
{
    public interface IEnterMessageInteractionHandler : IInteractionHandler
    {
        public SentFollowUpMessageModel GetSentFollowUpMessageModel();
    }
}
