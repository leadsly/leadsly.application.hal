using Leadsly.Application.Model.Requests;

namespace Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces
{
    public interface IGetMessageContentInteractionHandler : IInteractionHandler
    {
        public NewMessageRequest GetNewMessageRequest();
    }
}
