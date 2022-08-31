using Leadsly.Application.Model.Requests;
using System.Collections.Generic;

namespace Domain.Interactions.ScanProspectsForReplies.ScanProspects.Interfaces
{
    public interface IScanProspectsInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
        public IList<NewMessageRequest> NewMessageRequests { get; }
    }
}
