using Domain.Models.Networking;
using Domain.Models.Requests;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public IList<UpdateSearchUrlProgressRequest> GetUpdateSearchUrlRequests();
        public List<PersistPrimaryProspectRequest> GetPersistPrimaryProspectRequests();
        public IList<ConnectionSentRequest> GetConnectionSentRequests();
        void Execute(Leadsly.Application.Model.Campaigns.NetworkingMessageBody message, IList<SearchUrlProgress> searchUrlsProgress);
    }
}