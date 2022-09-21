using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<ConnectionSentModel> ConnectionsSent { get; }
        public bool GetMonthlySearchLimitReached();
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrlsProgress();
        void Execute(NetworkingMessageBody message, IList<SearchUrlProgressModel> searchUrlsProgress);
    }
}