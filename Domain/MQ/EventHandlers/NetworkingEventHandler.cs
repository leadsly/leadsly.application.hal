using Domain.Models.RabbitMQMessages;
using Domain.MQ.EventHandlers.Interfaces;
using Domain.PhaseHandlers.NetworkingHandler;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers
{
    public class NetworkingEventHandler : RabbitMQEventHandlerBase, INetworkingEventHandler
    {
        public NetworkingEventHandler(
            ILogger<NetworkingEventHandler> logger,
            HalWorkCommandHandlerDecorator<NetworkingCommand> networkingHandler)
            : base(logger)
        {
            _networkingHandler = networkingHandler;
            _logger = logger;
        }

        private readonly HalWorkCommandHandlerDecorator<NetworkingCommand> _networkingHandler;
        private readonly ILogger<NetworkingEventHandler> _logger;

        public async Task OnNetworkingEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string rawMessage = Encoding.UTF8.GetString(body);
            PublishMessageBody message = DeserializeMessage<NetworkingMessageBody>(rawMessage);

            NetworkingCommand networkingCommand = new NetworkingCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
            await _networkingHandler.HandleAsync(networkingCommand);
        }
    }
}
