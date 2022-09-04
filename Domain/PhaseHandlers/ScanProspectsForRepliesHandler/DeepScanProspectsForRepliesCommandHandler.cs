using Domain.Executors;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class DeepScanProspectsForRepliesCommandHandler : ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            IMessageExecutorHandler<DeepScanProspectsForRepliesBody> messageExecutorHandler
            )
        {
            _messageExecutorHandler = messageExecutorHandler;
            _logger = logger;
        }

        private IMessageExecutorHandler<DeepScanProspectsForRepliesBody> _messageExecutorHandler;
        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;

        public async Task HandleAsync(DeepScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            DeepScanProspectsForRepliesBody message = command.MessageBody as DeepScanProspectsForRepliesBody;
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
