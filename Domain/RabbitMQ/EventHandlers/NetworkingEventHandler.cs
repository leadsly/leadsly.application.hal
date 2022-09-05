using Domain.PhaseHandlers.NetworkingHandler;
using Domain.RabbitMQ.EventHandlers.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers
{
    public class NetworkingEventHandler : RabbitMQEventHandlerBase, INetworkingEventHandler
    {
        public NetworkingEventHandler(
            ILogger<NetworkingEventHandler> logger,
            HalWorkCommandHandlerDecorator<NetworkingCommand> networkingHandler)
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
            PublishMessageBody message = DeserializeMessage(rawMessage);

            NetworkingCommand networkingCommand = new NetworkingCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
            await _networkingHandler.HandleAsync(networkingCommand);
        }

        protected override PublishMessageBody DeserializeMessage(string rawMessage)
        {
            _logger.LogInformation("Deserializing FollowUpMessageBody");
            FollowUpMessageBody followUpMessageBody = null;
            try
            {
                followUpMessageBody = JsonConvert.DeserializeObject<FollowUpMessageBody>(rawMessage);
                _logger.LogDebug("Successfully deserialized FollowUpMessageBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize FollowUpMessageBody. Returning an explicit null");
                return null;
            }

            return followUpMessageBody;
        }
    }
}
