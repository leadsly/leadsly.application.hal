using Domain.RabbitMQ.Interfaces;
using Domain.Repositories;
using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace Domain.RabbitMQ
{
    public class RabbitMQManager : IRabbitMQManager
    {
        public RabbitMQManager(ILogger<RabbitMQManager> logger, IRabbitMQRepository rabbitMQRepository, ObjectPool<IModel> pool)
        {
            _rabbitMQRepository = rabbitMQRepository;
            _logger = logger;
            _pool = pool;
        }

        private readonly ObjectPool<IModel> _pool;
        private readonly ILogger<RabbitMQManager> _logger;
        private readonly IRabbitMQRepository _rabbitMQRepository;

        public void StartConsuming(string queueNameIn, string routingKeyIn, string halId, AsyncEventHandler<BasicDeliverEventArgs> receivedHandlerAsync)
        {
            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;

            _logger.LogInformation("Successfully created connection to RabbitMQ");
            var channel = _pool.Get();
            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", queueNameIn);

            IDictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add(RabbitMQConstants.QueueType, RabbitMQConstants.Classic);

            channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: arguments);

            string routingKey = options.RoutingKey.Replace("{halId}", halId);
            routingKey = routingKey.Replace("{purpose}", routingKeyIn);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += receivedHandlerAsync;

            // process only one message at a time
            channel.BasicQos(0, 1, false);

            channel.BasicConsume(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);

        }
    }
}
