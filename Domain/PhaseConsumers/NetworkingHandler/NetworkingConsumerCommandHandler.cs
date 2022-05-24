using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.NetworkingHandler
{
    public class NetworkingConsumerCommandHandler : IConsumeCommandHandler<NetworkingConsumerCommand>
    {
        public NetworkingConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IPhaseEventHandlerService campaignManagerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _campaignManagerService = campaignManagerService;
        }

        private readonly IPhaseEventHandlerService _campaignManagerService;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(NetworkingConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.Networking.QueueName;
            string routingKeyIn = RabbitMQConstants.Networking.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _campaignManagerService.OnNetworkingEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
