using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Networking;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        IList<ProspectRepliedModel> Prospects { get; }
        void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectModel> contactedProspects);
        void Execute(IWebDriver webDriver, DeepScanProspectsForRepliesBody message);
    }
}
