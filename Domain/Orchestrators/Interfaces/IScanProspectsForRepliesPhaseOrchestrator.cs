using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Models.RabbitMQMessages;

namespace Domain.Orchestrators.Interfaces
{
    public interface IScanProspectsForRepliesPhaseOrchestrator
    {
        event EndOfWorkDayReachedEventHandler EndOfWorkDayReached;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        void Execute(ScanProspectsForRepliesBody message);
    }
}
