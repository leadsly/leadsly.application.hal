using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.AllInOneVirtualAssistantHandler
{
    public class AllInOneVirtualAssistantConsumerCommandHandler : IConsumeCommandHandler<AllInOneVirtualAssistantConsumerCommand>
    {
        public AllInOneVirtualAssistantConsumerCommandHandler(
            IRabbitMQManager rabbitMQManager,
            IAllInOneVirtualAssistantEventHandler handler)
        {
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly IAllInOneVirtualAssistantEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(AllInOneVirtualAssistantConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.AllInOneVirtualAssistant.QueueName;
            string routingKeyIn = RabbitMQConstants.AllInOneVirtualAssistant.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnAllInOneVirtualAssistantEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
