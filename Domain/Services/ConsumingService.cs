using Domain.Repositories;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
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
        public ConsumingService(ILogger<ConsumingService> logger, IServiceProvider serviceProvider, IHalIdentity halIdentity, ICampaignManagerService campaignManagerService)
        {
            _halIdentity = halIdentity;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _campaignManagerService = campaignManagerService;
        }

        ~ConsumingService()
        {
            _logger.LogInformation("Destructing Comsuming Service");
            foreach (IModel channel in Channels)
            {
                try
                {
                    channel.Close();
                    channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error occured when closing rabbit mq channels");
                }
            }

            foreach (IConnection connection in Connections)
            {
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error occured when closing rabbit mq connections");
                }
            }
        }

        private readonly ILogger<ConsumingService> _logger;
        private readonly IHalIdentity _halIdentity;
        private readonly ICampaignManagerService _campaignManagerService;
        private readonly IServiceProvider _serviceProvider;
        private static readonly List<IModel> Channels = new();
        private static readonly List<IConnection> Connections = new();

        public void StartConsuming()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                RabbitMQOptions options = rabbitMQRepository.GetRabbitMQConfigOptions();
                string exchangeName = options.ExchangeOptions.Name;
                string exchangeType = options.ExchangeOptions.ExchangeType;

                // Combines the SendConnectionRequestsPhase and ProspectListPhase into a single queue
                // to ensure we are only ever executing one phase at a time
                NetworkingConnectionsQueue(options, exchangeName, exchangeType);
                FollowUpMessageQueue(options, exchangeName, exchangeType);
                MonitorForNewAcceptedConnectionsQueue(options, exchangeName, exchangeType);
                ScanProspectsForRepliesQueue(options, exchangeName, exchangeType);
                //ProspectListQueue(options, exchangeName, exchangeType);
                //SendConnectionRequestsQueue(options, exchangeName, exchangeType);
                SendEmailInvitesQueue(options, exchangeName, exchangeType);
                ConnectionWithdrawQueue(options, exchangeName, exchangeType);
                RescrapeSearchurlsQueue(options, exchangeName, exchangeType);
            }
        }

        private ConnectionFactory ConfigureConnectionFactor(RabbitMQOptions options, string clientProviderName, bool dispatchConsumerAsync = false)
        {
            return new ConnectionFactory()
            {
                UserName = options.ConnectionFactoryOptions.UserName,
                Password = options.ConnectionFactoryOptions.Password,
                HostName = options.ConnectionFactoryOptions.HostName,
                Port = options.ConnectionFactoryOptions.Port,
                ClientProvidedName = clientProviderName,
                DispatchConsumersAsync = dispatchConsumerAsync
            };
        }

        private void NetworkingConnectionsQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", RabbitMQConstants.NetworkingConnections.QueueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName, true);

            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
            name = name.Replace("{queueName}", RabbitMQConstants.NetworkingConnections.QueueName);

            channel.QueueDeclare(queue: name,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.NetworkingConnections.RoutingKey);
            channel.QueueBind(name, exchangeName, routingKey, null);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += _campaignManagerService.OnNetworkingConnectionsEventReceived;

            // process only one message at a time
            channel.BasicQos(0, 1, false);

            channel.BasicConsume(queue: name,
                                 autoAck: false,
                                 consumer: consumer);

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
            string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
            clientProviderName = clientProviderName.Replace("{queue}", RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);
            var factory = ConfigureConnectionFactor(options, clientProviderName);

            var connection = factory.CreateConnection();
            Connections.Add(connection);
            var channel = connection.CreateModel();
            Channels.Add(channel);

            channel.ExchangeDeclare(exchangeName, exchangeType);

            string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
            name = name.Replace("{queueName}", RabbitMQConstants.MonitorNewAcceptedConnections.QueueName);

            channel.QueueDeclare(queue: name,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

            string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
            routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey);
            channel.QueueBind(name, exchangeName, routingKey, null);

            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            consumer.Received += _campaignManagerService.OnMonitorForNewAcceptedConnectionsEventReceived;

            channel.BasicConsume(queue: name,
                                 autoAck: false,
                                 consumer: consumer);

        }

        private void ScanProspectsForRepliesQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        {
            const string queueName = RabbitMQConstants.ScanProspectsForReplies.QueueName;

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
                routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.ScanProspectsForReplies.RoutingKey);
                channel.QueueBind(name, exchangeName, routingKey, null);

                AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnScanProspectsForRepliesEventReceivedAsync;

                // process only one message at a time
                channel.BasicQos(0, 1, false);

                channel.BasicConsume(queue: name,
                                     autoAck: false,
                                     consumer: consumer);
            }
        }

        //private void ProspectListQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        //{
        //    string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
        //    clientProviderName = clientProviderName.Replace("{queue}", RabbitMQConstants.ProspectList.QueueName);
        //    var factory = ConfigureConnectionFactor(options, clientProviderName);

        //    var connection = factory.CreateConnection();
        //    Connections.Add(connection);
        //    var channel = connection.CreateModel();
        //    Channels.Add(channel);

        //    channel.ExchangeDeclare(exchangeName, exchangeType);

        //    string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
        //    name = name.Replace("{queueName}", RabbitMQConstants.ProspectList.QueueName);

        //    channel.QueueDeclare(queue: name,
        //                     durable: false,
        //                     exclusive: false,
        //                     autoDelete: false,
        //                     arguments: null);

        //    string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
        //    routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.ProspectList.RoutingKey);
        //    channel.QueueBind(name, exchangeName, routingKey, null);

        //    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        //    consumer.Received += _campaignManagerService.OnProspectListEventReceived;

        //    channel.BasicConsume(queue: name,
        //                         autoAck: false,
        //                         consumer: consumer);

        //}

        //private void SendConnectionRequestsQueue(RabbitMQOptions options, string exchangeName, string exchangeType)
        //{
        //    string clientProviderName = options.ConnectionFactoryOptions.ClientProvidedName.Replace("{halId}", this._halIdentity.Id);
        //    clientProviderName = clientProviderName.Replace("{queue}", RabbitMQConstants.SendConnections.QueueName);
        //    var factory = ConfigureConnectionFactor(options, clientProviderName);

        //    var connection = factory.CreateConnection();
        //    Connections.Add(connection);
        //    var channel = connection.CreateModel();
        //    Channels.Add(channel);

        //    channel.ExchangeDeclare(exchangeName, exchangeType);

        //    string name = options.QueueConfigOptions.Name.Replace("{halId}", this._halIdentity.Id);
        //    name = name.Replace("{queueName}", RabbitMQConstants.SendConnections.QueueName);

        //    channel.QueueDeclare(queue: name,
        //                     durable: false,
        //                     exclusive: false,
        //                     autoDelete: false,
        //                     arguments: null);

        //    string routingKey = options.RoutingKey.Replace("{halId}", this._halIdentity.Id);
        //    routingKey = routingKey.Replace("{purpose}", RabbitMQConstants.SendConnections.RoutingKey);
        //    channel.QueueBind(name, exchangeName, routingKey, null);

        //    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
        //    consumer.Received += _campaignManagerService.OnSendConnectionRequestsEventReceived;

        //    channel.BasicQos(0, 1, false);

        //    channel.BasicConsume(queue: name,
        //                         autoAck: false,
        //                         consumer: consumer);
        //}

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
