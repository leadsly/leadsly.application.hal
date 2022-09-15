using Domain.Models.RabbitMQMessages;
using Domain.MQ.EventHandlers.Interfaces;
using Domain.PhaseHandlers.FollowUpMessageHandlers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers
{
    public class FollowUpMessageEventHandler : RabbitMQEventHandlerBase, IFollowUpMessageEventHandler
    {
        public FollowUpMessageEventHandler(
            ILogger<FollowUpMessageEventHandler> logger,
            HalWorkCommandHandlerDecorator<FollowUpMessageCommand> followUpHandler)
            : base(logger)
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
            PublishMessageBody followUpMessages = DeserializeMessage<FollowUpMessageBody>(rawMessage);

            FollowUpMessageCommand followUpMessageCommand = new FollowUpMessageCommand(channel, eventArgs, followUpMessages, followUpMessages.StartOfWorkday, followUpMessages.EndOfWorkday, followUpMessages.TimeZoneId);
            await _followUpHandler.HandleAsync(followUpMessageCommand);
        }


    }
}
