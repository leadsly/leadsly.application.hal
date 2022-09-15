using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.NetworkingHandler
{
    public class NetworkingConsumerCommandHandler : IConsumeCommandHandler<NetworkingConsumerCommand>
    {
        public NetworkingConsumerCommandHandler(IRabbitMQManager rabbitMQManager, INetworkingEventHandler handler)
        {
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly INetworkingEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(NetworkingConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.Networking.QueueName;
            string routingKeyIn = RabbitMQConstants.Networking.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnNetworkingEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
