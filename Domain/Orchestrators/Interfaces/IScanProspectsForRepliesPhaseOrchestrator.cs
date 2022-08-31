using Domain.Executors.ScanProspectsForReplies.Events;

namespace Domain.Orchestrators.Interfaces
{
    public interface IScanProspectsForRepliesPhaseOrchestrator
    {
        event EndOfWorkDayReachedEventHandler EndOfWorkDayReached;
        event NewMessagesReceivedEventHandler NewMessagesReceived;
        void Execute(Leadsly.Application.Model.Campaigns.ScanProspectsForRepliesBody message);
    }
}
