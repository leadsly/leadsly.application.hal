using Domain.Interactions.Networking.ConnectWithProspect.Interfaces;
using Domain.Models.Requests;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.Decorators
{
    public class RetryConnectWithProspectHandlerDecorator : IConnectWithProspectInteractionHandler
    {
        public RetryConnectWithProspectHandlerDecorator(ILogger<RetryConnectWithProspectHandlerDecorator> logger, IConnectWithProspectInteractionHandler decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }

        public ConnectionSentRequest ConnectionSentRequest => _decorated.ConnectionSentRequest;
        private readonly ILogger<RetryConnectWithProspectHandlerDecorator> _logger;
        private readonly IConnectWithProspectInteractionHandler _decorated;

        public bool HandleInteraction(InteractionBase interaction)
        {
            bool succeeded = _decorated.HandleInteraction(interaction);
            if (succeeded == false)
            {
                // handle re tries here if we wanted to                
            }

            return succeeded;
        }
    }
}
