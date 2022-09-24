using Domain.Models.ScanProspectsForReplies;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessagesContent.Interfaces
{
    public interface IGetUnreadMessagesContentInteractionHandler : IInteractionHandler
    {
        public IList<NewMessageModel> GetNewMessages();
    }
}
