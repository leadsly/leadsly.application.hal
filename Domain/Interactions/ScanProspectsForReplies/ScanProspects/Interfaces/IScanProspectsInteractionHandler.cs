using Leadsly.Application.Model.Requests;
using System.Collections.Generic;

namespace Domain.Interactions.ScanProspectsForReplies.ScanProspects.Interfaces
{
    public interface IScanProspectsInteractionHandler : IInteractionHandler
    {
        public IList<NewMessageRequest> GetNewMessageRequests();
    }
}
