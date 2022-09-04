using Domain.Models;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces
{
    public interface ICheckMessagesHistoryInteractionHandler : IInteractionHandler
    {
        public ProspectReplied GetProspect();
    }
}
