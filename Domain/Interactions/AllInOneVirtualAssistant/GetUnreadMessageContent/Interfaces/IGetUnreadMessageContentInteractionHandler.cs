using Domain.Models.ScanProspectsForReplies;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessageContent.Interfaces
{
    public interface IGetUnreadMessageContentInteractionHandler : IInteractionHandler
    {
        public NewMessageModel GetNewMessage();
    }
}
