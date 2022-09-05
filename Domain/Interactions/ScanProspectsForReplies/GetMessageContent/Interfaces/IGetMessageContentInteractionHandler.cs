using Domain.Models.ScanProspectsForReplies;

namespace Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces
{
    public interface IGetMessageContentInteractionHandler : IInteractionHandler
    {
        public NewMessage GetNewMessage();
    }
}
