using Domain.Models.DeepScanProspectsForReplies;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces
{
    public interface ICheckMessagesHistoryInteractionHandler : IInteractionHandler
    {
        public ProspectRepliedModel GetProspect();
    }
}
