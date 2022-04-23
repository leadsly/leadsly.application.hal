using Domain.Repositories;
using Leadsly.Application.Model.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class RabbitMQManager : IRabbitMQManager
    {
        public RabbitMQManager(IRabbitMQRepository rabbitMQRepository)
        {
            _rabbitMQRepository = rabbitMQRepository;
        }

        private readonly IRabbitMQRepository _rabbitMQRepository;
        private static readonly List<IModel> Channels = new();
        private static readonly List<IConnection> Connections = new();

        public void StartConsuming(string queueNameIn, string routingKeyIn, string halId, AsyncEventHandler<BasicDeliverEventArgs> receivedHandlerAsync)
        {
            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", halId);
            clientProviderName = clientProviderName.Replace("{queue}", queueNameIn);
            var factory = ConfigureConnectionFactory(options, clientProviderName, true);

            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string queueName = options.QueueConfigOptions.Name.Replace("{halId}", halId);
            queueName = queueName.Replace("{queueName}", queueNameIn);

            channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

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

        private ConnectionFactory ConfigureConnectionFactory(RabbitMQOptions options, string clientProviderName, bool dispatchConsumersAsync = false)
        {
            return new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryOptions.UserName,
                Password = options.ConnectionFactoryOptions.Password,
                HostName = options.ConnectionFactoryOptions.HostName,
                Port = options.ConnectionFactoryOptions.Port,
                ClientProvidedName = clientProviderName,
                DispatchConsumersAsync = dispatchConsumersAsync
            };
        }
    }
}
