using Domain.Models;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces
{
    public interface ICheckMessagesHistoryInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
        public ProspectReplied GetProspect();
    }
}
