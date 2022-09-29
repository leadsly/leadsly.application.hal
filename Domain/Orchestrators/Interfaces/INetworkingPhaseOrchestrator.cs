using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.Networking;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects;
        public event ConnectionsSentEventHandler ConnectionsSent;
        public event MonthlySearchLimitReachedEventHandler SearchLimitReached;
        public event UpdatedSearchUrlProgressEventHandler UpdatedSearchUrlsProgress;
        void Execute(NetworkingMessageBody message, IList<SearchUrlProgressModel> searchUrlsProgress);
        void Execute(IWebDriver webdriver, AllInOneVirtualAssistantMessageBody message);
    }
}