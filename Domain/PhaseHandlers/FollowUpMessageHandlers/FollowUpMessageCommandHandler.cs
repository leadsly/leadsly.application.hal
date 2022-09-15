using Domain.Executors;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.FollowUpMessageHandlers
{
    public class FollowUpMessageCommandHandler : ICommandHandler<FollowUpMessageCommand>
    {
        public FollowUpMessageCommandHandler(
            IMessageExecutorHandler<FollowUpMessageBody> messageExecutorHandler,
            ILogger<FollowUpMessageCommandHandler> logger)
        {
            _messageExecutorHandler = messageExecutorHandler;
            _logger = logger;
        }

        private readonly ILogger<FollowUpMessageCommandHandler> _logger;
        private readonly IMessageExecutorHandler<FollowUpMessageBody> _messageExecutorHandler;

        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            FollowUpMessageBody message = command.MessageBody as FollowUpMessageBody;
            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);

            if (succeeded == true)
            {
                _logger.LogDebug($"{nameof(FollowUpMessageBody)} phase finished executing successfully");
            }
            else
            {
                _logger.LogDebug($"{nameof(FollowUpMessageBody)} phase finished executing unsuccessfully");
            }
        }
    }
}
