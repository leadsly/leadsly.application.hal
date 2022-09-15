using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IFollowUpMessagePhaseOrchestrator
    {
        public SentFollowUpMessageModel GetSentFollowUpMessage();
        void Execute(FollowUpMessageBody message);
    }
}
