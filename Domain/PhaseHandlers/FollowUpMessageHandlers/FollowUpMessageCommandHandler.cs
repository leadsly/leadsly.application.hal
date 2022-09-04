using Domain.Executors;
using Leadsly.Application.Model.Campaigns;
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
            messageExecutorHandler = _messageExecutorHandler;
            _logger = logger;
        }

        private readonly ILogger<FollowUpMessageCommandHandler> _logger;
        private IMessageExecutorHandler<FollowUpMessageBody> _messageExecutorHandler;

        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            FollowUpMessageBody message = command.MessageBody as FollowUpMessageBody;
            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);

            if (succeeded == true)
            {
                _logger.LogDebug($"DeepScanProspectsForReplies phase finishex executing successfully");
            }
            else
            {
                _logger.LogDebug($"DeepScanProspectsForReplies phase finishex executing unsuccessfully");
            }
        }
    }
}
