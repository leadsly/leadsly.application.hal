using Domain.Models.FollowUpMessage;
using Domain.Models.RabbitMQMessages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IFollowUpMessagePhaseOrchestrator
    {
        public SentFollowUpMessage GetSentFollowUpMessage();
        void Execute(FollowUpMessageBody message);
    }
}
