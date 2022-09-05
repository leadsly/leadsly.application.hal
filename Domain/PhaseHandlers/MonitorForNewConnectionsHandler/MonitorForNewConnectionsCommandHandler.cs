using Domain.Executors;
using Domain.Models.RabbitMQMessages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class MonitorForNewConnectionsCommandHandler : ICommandHandler<MonitorForNewConnectionsCommand>
    {
        public MonitorForNewConnectionsCommandHandler(
            ILogger<MonitorForNewConnectionsCommandHandler> logger,
            IMessageExecutorHandler<MonitorForNewAcceptedConnectionsBody> messageExecutorHandler
            )
        {
            _messageExecutorHandler = messageExecutorHandler;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsCommandHandler> _logger;
        private readonly IMessageExecutorHandler<MonitorForNewAcceptedConnectionsBody> _messageExecutorHandler;

        public async Task HandleAsync(MonitorForNewConnectionsCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            MonitorForNewAcceptedConnectionsBody message = command.MessageBody as MonitorForNewAcceptedConnectionsBody;
            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);

            if (succeeded == true)
            {
                _logger.LogDebug($"{nameof(MonitorForNewAcceptedConnectionsBody)} phase finished executing successfully");
            }
            else
            {
                _logger.LogDebug($"{nameof(MonitorForNewAcceptedConnectionsBody)} phase finished executing unsuccessfully");
            }
        }
    }
}
