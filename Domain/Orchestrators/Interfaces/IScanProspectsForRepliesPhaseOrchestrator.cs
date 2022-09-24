using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.MQ.Messages;
using OpenQA.Selenium;

namespace Domain.Orchestrators.Interfaces
{
    public interface IScanProspectsForRepliesPhaseOrchestrator
    {
        event EndOfWorkDayReachedEventHandler EndOfWorkDayReached;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        void Execute(ScanProspectsForRepliesBody message);
        void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
    }
}
