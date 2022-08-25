using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.ConnectWithProspect.Interfaces;
using Domain.Models.Requests;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.Decorators
{
    public class RetryConnectWithProspectHandlerDecorator<TInteraction> : IConnectWithProspectInteractionHandler<TInteraction>
        where TInteraction : ConnectWithProspectInteraction
    {
        public RetryConnectWithProspectHandlerDecorator(ILogger<RetryConnectWithProspectHandlerDecorator<TInteraction>> logger, IConnectWithProspectInteractionHandler<TInteraction> decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }

        public ConnectionSentRequest ConnectionSentRequest => _decorated.ConnectionSentRequest;
        private readonly ILogger<RetryConnectWithProspectHandlerDecorator<TInteraction>> _logger;
        private readonly IConnectWithProspectInteractionHandler<TInteraction> _decorated;

        public bool HandleInteraction(TInteraction interaction)
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
