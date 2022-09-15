using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.RestartApplicationHandler
{
    public class RestartApplicationConsumerCommandHandler : IConsumeCommandHandler<RestartApplicationConsumerCommand>
    {
        public RestartApplicationConsumerCommandHandler(
            ILogger<RestartApplicationConsumerCommandHandler> logger,
            IRabbitMQManager rabbitMQManager,
            IRestartApplicationEventHandler handler)
        {
            _logger = logger;
            _rabbitMQManager = rabbitMQManager;
            _handler = handler;
        }

        private readonly ILogger<RestartApplicationConsumerCommandHandler> _logger;
        private readonly IRestartApplicationEventHandler _handler;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(RestartApplicationConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.RestartApplication.QueueName;
            string routingKeyIn = RabbitMQConstants.RestartApplication.RoutingKey;
            string halId = command.HalId;
            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _handler.OnRestartApplicationEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
