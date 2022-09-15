using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsConsumerCommandHandler : IConsumeCommandHandler<MonitorForNewConnectionsConsumerCommand>
    {
        public MonitorForNewConnectionsConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IMonitorForNewAcceptedConnectionsEventHandler handler)
        {
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly IMonitorForNewAcceptedConnectionsEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(MonitorForNewConnectionsConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnMonitorForNewAcceptedConnectionsEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
