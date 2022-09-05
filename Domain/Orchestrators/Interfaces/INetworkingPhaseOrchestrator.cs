using Domain.Models.ProspectList;
using Domain.Models.RabbitMQMessages;
using Domain.Models.SendConnections;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public IList<Models.Networking.SearchUrlProgress> GetUpdatedSearchUrls();
        public List<PersistPrimaryProspect> GetPersistPrimaryProspects();
        public IList<ConnectionSent> GetConnectionsSent();
        public bool GetMonthlySearchLimitReached();
        void Execute(NetworkingMessageBody message, IList<Models.Networking.SearchUrlProgress> searchUrlsProgress);
    }
}