using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Networking;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        IList<ProspectRepliedModel> Prospects { get; }
        void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectModel> contactedProspects);
    }
}
