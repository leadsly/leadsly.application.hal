using Domain.Repositories;
using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ConsumingService : IConsumingService
    {
        public ConsumingService(ILogger<ConsumingService> logger, IRabbitMQRepository rabbitMQRepository, IHalIdentity halIdentity, ICampaignManagerService campaignManagerService)
        {
            _halIdentity = halIdentity;
            _logger = logger;
            _campaignManagerService = campaignManagerService;
            _rabbitMQRepository = rabbitMQRepository;
        }

        ~ConsumingService()
        {
            _logger.LogInformation("Destructing Comsuming Service");
            foreach (IModel channel in Channels)
            {
                channel.Close();
                channel.Dispose();
            }

            foreach (IConnection connection in Connections)
            {
                connection.Close();
                connection.Dispose();
            }
        }

        private readonly ILogger<ConsumingService> _logger;
        private readonly IHalIdentity _halIdentity;
        private readonly ICampaignManagerService _campaignManagerService;
        private readonly IRabbitMQRepository _rabbitMQRepository;
        private readonly List<IModel> Channels = new();
        private readonly List<IConnection> Connections = new();

        public void StartConsuming()
        {
            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();
            string exchangeName = options.ExchangeOptions.Name;
            string exchangeType = options.ExchangeOptions.ExchangeType;

            FollowUpMessageQueue(options, exchangeName, exchangeType);
            MonitorForNewAcceptedConnectionsQueue(options, exchangeName, exchangeType);
            ScanProspectsForRepliesQueue(options, exchangeName, exchangeType);
            ProspectListQueue(options, exchangeName, exchangeType);
            SendConnectionRequestsQueue(options, exchangeName, exchangeType);
            SendEmailInvitesQueue(options, exchangeName, exchangeType);
            ConnectionWithdrawQueue(options, exchangeName, exchangeType);
            RescrapeSearchurlsQueue(options, exchangeName, exchangeType);
        }

        private ConnectionFactory ConfigureConnectionFactor(RabbitMQOptions options, string clientProviderName)
        {
            return new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryOptions.UserName,
                Password = options.ConnectionFactoryOptions.Password,
                HostName = options.ConnectionFactoryOptions.HostName,
                Port = options.ConnectionFactoryOptions.Port,
                ClientProvidedName = clientProviderName
            };
        }

        private void FollowUpMessageQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "follow.up.message";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
            name = name.Replace("{queueName}", queueName);

            channel.QueueDeclare(queue: name,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
            routingKey = routingKey.Replace("{purpose}", "follow-up-messages");
            channel.QueueBind(name, exchangeName, routingKey, null);

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += _campaignManagerService.OnFollowUpMessageEventReceived;

            channel.BasicConsume(queue: name,
                                 autoAck: false,
                                 consumer: consumer);

        }

        private void MonitorForNewAcceptedConnectionsQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "monitor.for.new.connections";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "monitor-new-connections");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnMonitorForNewAcceptedConnectionsEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void ScanProspectsForRepliesQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "scan.prospects.for.replies";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "scan-for-replies");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnScanProspectsForRepliesEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void ProspectListQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "prospect.list";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "prospect-list");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnProspectListEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void SendConnectionRequestsQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "send.connection.requests";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "send-connection-requests");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnSendConnectionRequestsEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void SendEmailInvitesQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "send.email.invites";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "send-email-invites");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnSendEmailInvitesEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void ConnectionWithdrawQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = "connection.withdraw";

            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", queueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
                name = name.Replace("{queueName}", queueName);

                channel.QueueDeclare(queue: name,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
                routingKey = routingKey.Replace("{purpose}", "withdraw-connections");
                channel.QueueBind(name, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnConnectionWithdrawEventReceived;

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        private void RescrapeSearchurlsQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {

        }
    }
}
