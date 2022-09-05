using Domain.PhaseHandlers.FollowUpMessageHandlers;
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
    public class FollowUpMessageEventHandler : RabbitMQEventHandlerBase, IFollowUpMessageEventHandler
    {
        public FollowUpMessageEventHandler(
            ILogger<FollowUpMessageEventHandler> logger,
            HalWorkCommandHandlerDecorator<FollowUpMessageCommand> followUpHandler)
        {
            _logger = logger;
            _followUpHandler = followUpHandler;
        }

        private readonly ILogger<FollowUpMessageEventHandler> _logger;
        private readonly HalWorkCommandHandlerDecorator<FollowUpMessageCommand> _followUpHandler;

        public async Task OnFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string rawMessage = Encoding.UTF8.GetString(body);
            PublishMessageBody followUpMessages = DeserializeMessage(rawMessage);

            FollowUpMessageCommand followUpMessageCommand = new FollowUpMessageCommand(channel, eventArgs, followUpMessages, followUpMessages.StartOfWorkday, followUpMessages.EndOfWorkday, followUpMessages.TimeZoneId);
            await _followUpHandler.HandleAsync(followUpMessageCommand);
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
