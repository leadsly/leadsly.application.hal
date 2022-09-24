using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.MQ.Messages;
using OpenQA.Selenium;

namespace Domain.Orchestrators.Interfaces
{
    public interface IMonitorForNewConnectionsPhaseOrchestrator
    {
        event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects;
        void Execute(MonitorForNewAcceptedConnectionsBody message);
        void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
    }
}
