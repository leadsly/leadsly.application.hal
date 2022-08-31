using Domain.Models;
using Domain.Models.Responses;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        IList<ProspectReplied> Prospects { get; }
        void Execute(Leadsly.Application.Model.Campaigns.DeepScanProspectsForRepliesBody message, IList<NetworkProspectResponse> contactedProspects);
    }
}
