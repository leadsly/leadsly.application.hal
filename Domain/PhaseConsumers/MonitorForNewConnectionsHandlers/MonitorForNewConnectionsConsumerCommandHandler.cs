using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsConsumerCommandHandler : IConsumeCommandHandler<MonitorForNewConnectionsConsumerCommand>
    {
        public MonitorForNewConnectionsConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IPhaseEventHandlerService campaignManagerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _campaignManagerService = campaignManagerService;
        }

        private readonly IPhaseEventHandlerService _campaignManagerService;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(MonitorForNewConnectionsConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _campaignManagerService.OnMonitorForNewAcceptedConnectionsEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
