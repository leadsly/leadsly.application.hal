using Domain.Executors;
using Domain.RabbitMQ;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class CheckOffHoursNewConnectionsCommandHandler : ICommandHandler<CheckOffHoursNewConnectionsCommand>
    {
        public CheckOffHoursNewConnectionsCommandHandler(
            ILogger<CheckOffHoursNewConnectionsCommandHandler> logger,
            IMessageExecutorHandler<CheckOffHoursNewConnectionsBody> messageExecutorHandler
            )
        {
            _messageExecutorHandler = messageExecutorHandler;
            _logger = logger;
        }

        private readonly ILogger<CheckOffHoursNewConnectionsCommandHandler> _logger;
        private readonly IMessageExecutorHandler<CheckOffHoursNewConnectionsBody> _messageExecutorHandler;

        public async Task HandleAsync(CheckOffHoursNewConnectionsCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            CheckOffHoursNewConnectionsBody message = command.MessageBody as CheckOffHoursNewConnectionsBody;

            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);
            if (succeeded == true)
            {
                _logger.LogInformation($"Positively acknowledging {nameof(CheckOffHoursNewConnectionsBody)}");
                channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            else
            {
                _logger.LogInformation($"Negatively acknowledging {nameof(CheckOffHoursNewConnectionsBody)}");
                channel.BasicNackRetry(eventArgs);
            }
        }
    }
}
