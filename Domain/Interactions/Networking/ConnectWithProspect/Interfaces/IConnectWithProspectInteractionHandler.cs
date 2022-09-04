using Domain.Models.Requests;

namespace Domain.Interactions.Networking.ConnectWithProspect.Interfaces
{
    public interface IConnectWithProspectInteractionHandler : IInteractionHandler
    {
        public ConnectionSentRequest ConnectionSentRequest { get; }
    }
}
