using Domain.Models.FollowUpMessage;
using Domain.Models.RabbitMQMessages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IFollowUpMessagePhaseOrchestrator
    {
        public SentFollowUpMessageModel GetSentFollowUpMessage();
        void Execute(FollowUpMessageBody message);
    }
}
