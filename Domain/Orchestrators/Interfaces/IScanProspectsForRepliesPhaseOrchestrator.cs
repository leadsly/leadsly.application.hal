using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.MQ.Messages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IScanProspectsForRepliesPhaseOrchestrator
    {
        event EndOfWorkDayReachedEventHandler EndOfWorkDayReached;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        void Execute(ScanProspectsForRepliesBody message);
    }
}
