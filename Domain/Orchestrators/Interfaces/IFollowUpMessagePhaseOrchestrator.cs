using Domain.Models.Requests;
using Leadsly.Application.Model.Campaigns;

namespace Domain.Orchestrators.Interfaces
{
    public interface IFollowUpMessagePhaseOrchestrator
    {
        public SentFollowUpMessageRequest GetSentFollowUpMessage();
        void Execute(FollowUpMessageBody message);
    }
}
