using Domain.RabbitMQ.EventHandlers.Interfaces;
using Domain.RabbitMQ.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.FollowUpMessageHandlers
{
    public class FollowUpMessageConsumerCommandHandler : IConsumeCommandHandler<FollowUpMessageConsumerCommand>
    {
        public FollowUpMessageConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IFollowUpMessageEventHandler handler)
        {
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly IFollowUpMessageEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;
        public Task ConsumeAsync(FollowUpMessageConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
            string routingKeyIn = RabbitMQConstants.FollowUpMessage.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnFollowUpMessageEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
