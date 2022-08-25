using Domain.Executors;
using Domain.Facades.Interfaces;
using Domain.Models.Networking;
using Domain.RabbitMQ;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
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
                _logger.LogInformation("Networking phase executed successfully. Acknowledging message");
                channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            else
            {
                _logger.LogWarning("Networking phase did not successfully execute. Negatively acknowledging the message. This message will not be re-queued");
                channel.BasicNackRetry(eventArgs);
            }
        }
    }
}
