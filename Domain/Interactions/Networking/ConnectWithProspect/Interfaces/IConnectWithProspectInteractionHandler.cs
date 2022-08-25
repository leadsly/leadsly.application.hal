using Domain.Models.Requests;
using System.Collections.Generic;

namespace Domain.Interactions.Networking.ConnectWithProspect.Interfaces
{
    public interface IConnectWithProspectInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
        public ConnectionSentRequest ConnectionSentRequest { get; }
    }
}
