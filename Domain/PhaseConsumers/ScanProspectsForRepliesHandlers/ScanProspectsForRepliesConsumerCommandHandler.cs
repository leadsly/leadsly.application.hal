using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesConsumerCommandHandler : IConsumeCommandHandler<ScanProspectsForRepliesConsumerCommand>
    {
        public ScanProspectsForRepliesConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IScanProspectsForRepliesEventHandler handler)
        {
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly IScanProspectsForRepliesEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(ScanProspectsForRepliesConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = command.HalId;

            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnScanProspectsForRepliesEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
