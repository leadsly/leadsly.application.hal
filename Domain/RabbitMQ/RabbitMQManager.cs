using Domain.RabbitMQ.Interfaces;
using Domain.Repositories;
using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace Domain.RabbitMQ
{
    public class RabbitMQManager : IRabbitMQManager
    {
        public RabbitMQManager(ILogger<RabbitMQManager> logger, IRabbitMQRepository rabbitMQRepository, IMemoryCache memoryCache, ObjectPool<IModel> pool)
        {
            _rabbitMQRepository = rabbitMQRepository;
            _logger = logger;
            _pool = pool;
            _memoryCache = memoryCache;
        }

        private readonly IMemoryCache _memoryCache;
        private readonly ObjectPool<IModel> _pool;
        private readonly ILogger<RabbitMQManager> _logger;
        private readonly IRabbitMQRepository _rabbitMQRepository;

        public void StartConsuming(string queueNameIn, string routingKeyIn, string halId, AsyncEventHandler<BasicDeliverEventArgs> receivedHandlerAsync)
        {
            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();
            string exchangeName = options.ExchangeOptions.Hal.Name;
            string exchangeType = options.ExchangeOptions.Hal.ExchangeType;

            _logger.LogInformation("Successfully created connection to RabbitMQ");
            var channel = _pool.Get();
            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Hal.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", queueNameIn);

            IDictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add(RabbitMQConstants.QueueType, RabbitMQConstants.Classic);

            channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: true,
                             arguments: arguments);

            string routingKey = options.RoutingKey.Hal.Replace("{halId}", halId);
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

        public void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn)
        {
            RabbitMQOptions options = GetRabbitMQOptions();

            string exchangeName = options.ExchangeOptions.AppServer.Name;
            string exchangeType = options.ExchangeOptions.AppServer.ExchangeType;

            IModel channel = _pool.Get();
            channel.ExchangeDeclare(exchangeName, exchangeType);

            IDictionary<string, object> arguments = new Dictionary<string, object>();
            arguments.Add(RabbitMQConstants.QueueType, RabbitMQConstants.Classic);

            channel.QueueDeclare(queue: queueNameIn, durable: true, exclusive: false, autoDelete: true, arguments: arguments);

            channel.QueueBind(queueNameIn, exchangeName, routingKeyIn, null);

            _logger.LogInformation("The exchangeName is: {exchangeName} " +
                            "\r\nThe exchangeType is: {exchangeType} ",
                            exchangeName,
                            exchangeType);

            channel.BasicPublish(exchange: exchangeName, routingKey: routingKeyIn, basicProperties: null, body: body);

            _pool.Return(channel);
        }

        private RabbitMQOptions GetRabbitMQOptions()
        {
            RabbitMQOptions options = default;
            if (_memoryCache.TryGetValue(CacheKeys.RabbitMQConfigOptions, out options) == false)
            {
                options = _rabbitMQRepository.GetRabbitMQConfigOptions();

                _memoryCache.Set(CacheKeys.RabbitMQConfigOptions, options, DateTimeOffset.Now.AddMinutes(10));
            }
            return options;
        }
    }
}
