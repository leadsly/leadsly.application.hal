using Domain.Models.SendConnections;

namespace Domain.Interactions.Networking.ConnectWithProspect.Interfaces
{
    public interface IConnectWithProspectInteractionHandler : IInteractionHandler
    {
        public ConnectionSentModel ConnectionSent { get; }
    }
}
