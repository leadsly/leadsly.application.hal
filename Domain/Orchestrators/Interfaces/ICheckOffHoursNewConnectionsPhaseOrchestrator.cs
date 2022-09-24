using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface ICheckOffHoursNewConnectionsPhaseOrchestrator
    {
        event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected;
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        void Execute(CheckOffHoursNewConnectionsBody message);
        void Execute(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message);
    }
}
