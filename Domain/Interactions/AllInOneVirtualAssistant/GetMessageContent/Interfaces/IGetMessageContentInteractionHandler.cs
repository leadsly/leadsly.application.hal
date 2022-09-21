using Domain.Models.ScanProspectsForReplies;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent.Interfaces
{
    public interface IGetMessageContentInteractionHandler : IInteractionHandler
    {
        public NewMessageModel GetNewMessage();
    }
}
