using Domain.Models.ScanProspectsForReplies;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent.Interfaces
{
    public interface IGetMessagesContentInteractionHandler : IInteractionHandler
    {
        public IList<NewMessageModel> GetNewMessages();
    }
}
