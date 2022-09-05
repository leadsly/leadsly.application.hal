using Domain.Executors;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class ScanProspectsForRepliesCommandHandler : ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            ILogger<ScanProspectsForRepliesCommandHandler> logger,
            IMessageExecutorHandler<ScanProspectsForRepliesBody> messageExecutorHandler
            )
        {
            _logger = logger;
            _messageExecutorHandler = messageExecutorHandler;
        }

        private readonly IMessageExecutorHandler<ScanProspectsForRepliesBody> _messageExecutorHandler;
        private readonly ILogger<ScanProspectsForRepliesCommandHandler> _logger;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            // acknowledge the message right away. Let hangfire handle retryies
            channel.BasicAck(eventArgs.DeliveryTag, false);

            ScanProspectsForRepliesBody message = command.MessageBody as ScanProspectsForRepliesBody;
            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);
            if (succeeded == true)
            {
                _logger.LogDebug($"{nameof(ScanProspectsForRepliesBody)} phase finished executing successfully");
            }
            else
            {
                _logger.LogDebug($"{nameof(ScanProspectsForRepliesBody)} phase finished executing unsuccessfully");
            }
        }

    }
}
