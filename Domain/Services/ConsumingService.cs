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

        private readonly ILogger<ConsumingService> _logger;
        private readonly IHalIdentity _halIdentity;
        private readonly ICampaignManagerService _campaignManagerService;
        private readonly IRabbitMQRepository _rabbitMQRepository;

        public void StartConsuming()
        {
            RabbitMQOptions options = _rabbitMQRepository.GetRabbitMQConfigOptions();

            FollowUpMessageQueue(options);
            MonitorForNewAcceptedConnectionsQueue();
            ScanProspectsForRepliesQueue();
            ProspectListQueue();
            SendConnectionRequestsQueue();
            SendEmailInvitesQueue();
            ConnectionWithdrawQueue();
            RescrapeSearchurlsQueue();

            var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "prospect-list-queue-hal123",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                var consumer = new EventingBasicConsumer(channel);                
                consumer.Received += (model, ea) =>
                {
                    // manually acknowledge of the message being received
                    ((IModel)model).BasicAck(ea.DeliveryTag, false);
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                };
                channel.BasicConsume(queue: "prospect-list-queue-hal123",                  
                                     autoAck: true,
                                     consumer: consumer);

               
            }
                        
        }

        private void FollowUpMessageQueue(RabbitMQOptions options)
        {
            var factory = new ConnectionFactory()
            {
                UserName = "guest",
                Password = "guest",
                HostName = "localhost",
                Port = 5672,
                ClientProvidedName = $"hallId:{_halIdentity.Id} queue:follow-up-message-queue"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchangeName, exchangeType);

                channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                channel.QueueBind(queue: queueName, exchangeName, routingKey, null);

                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += _campaignManagerService.OnFollowUpMessageEventReceived;

                channel.BasicConsume(queue: queueName,
                                     autoAck: false,
                                     consumer: consumer);

                
            }
        }

        private void MonitorForNewAcceptedConnectionsQueue()
        {

        }

        private void ScanProspectsForRepliesQueue()
        {

        }

        private void ProspectListQueue()
        {

        }

        private void SendConnectionRequestsQueue()
        {

        }

        private void SendEmailInvitesQueue()
        {

        }

        private void ConnectionWithdrawQueue()
        {

        }

        private void RescrapeSearchurlsQueue()
        {

        }
    }
}
