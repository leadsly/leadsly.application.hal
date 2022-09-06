using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.RabbitMQMessages;
using Domain.Models.Responses;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        IList<ProspectRepliedModel> Prospects { get; }
        void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectResponse> contactedProspects);
    }
}
