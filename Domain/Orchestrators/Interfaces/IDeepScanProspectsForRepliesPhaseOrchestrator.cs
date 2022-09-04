using Domain.Models;
using Domain.Models.Responses;
using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        IList<ProspectReplied> Prospects { get; }
        void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectResponse> contactedProspects);
    }
}
