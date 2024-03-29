﻿using Domain.Executors;
using Domain.MQ;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.NetworkingHandler
{
    public class NetworkingCommandHandler : ICommandHandler<NetworkingCommand>
    {
        public NetworkingCommandHandler(
            ILogger<NetworkingCommandHandler> logger,
            IMessageExecutorHandler<NetworkingMessageBody> messageExecutorHandler
            )
        {
            _logger = logger;
            _messageExecutorHandler = messageExecutorHandler;
        }

        private readonly ILogger<NetworkingCommandHandler> _logger;
        private readonly IMessageExecutorHandler<NetworkingMessageBody> _messageExecutorHandler;

        public async Task HandleAsync(NetworkingCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            NetworkingMessageBody message = command.MessageBody as NetworkingMessageBody;

            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);
            if (succeeded == true)
            {
                _logger.LogInformation($"Positively acknowledging {nameof(NetworkingMessageBody)}");
                channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            else
            {
                _logger.LogInformation($"Negatively acknowledging {nameof(NetworkingMessageBody)}");
                channel.BasicNackRetry(eventArgs);
            }
        }
    }
}
