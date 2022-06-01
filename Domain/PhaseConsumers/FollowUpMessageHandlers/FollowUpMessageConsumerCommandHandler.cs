using Domain.RabbitMQ.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.FollowUpMessageHandlers
{
    public class FollowUpMessageConsumerCommandHandler : IConsumeCommandHandler<FollowUpMessageConsumerCommand>
    {
        public FollowUpMessageConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IPhaseEventHandlerService campaignManagerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _campaignManagerService = campaignManagerService;
        }

        private readonly IPhaseEventHandlerService _campaignManagerService;
        private readonly IRabbitMQManager _rabbitMQManager;
        public Task ConsumeAsync(FollowUpMessageConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
            string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _campaignManagerService.OnFollowUpMessageEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
