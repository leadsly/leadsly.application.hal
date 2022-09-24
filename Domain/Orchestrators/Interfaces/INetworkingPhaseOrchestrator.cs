using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<ConnectionSentModel> ConnectionsSent { get; }
        public bool GetMonthlySearchLimitReached();
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; }
        void Execute(NetworkingMessageBody message, IList<SearchUrlProgressModel> searchUrlsProgress);
        void Execute(IWebDriver webdriver, AllInOneVirtualAssistantMessageBody message);
    }
}