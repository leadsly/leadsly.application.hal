using Domain.Executors;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.AllInOneVirtualAssistantHandler
{
    public class AllInOneVirtualAssistantCommandHandler : ICommandHandler<AllInOneVirtualAssistantCommand>
    {
        public AllInOneVirtualAssistantCommandHandler(
            ILogger<AllInOneVirtualAssistantCommandHandler> logger,
            IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody> handler)
        {
            _logger = logger;
            _handler = handler;
        }

        private readonly ILogger<AllInOneVirtualAssistantCommandHandler> _logger;
        private readonly IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody> _handler;

        public async Task HandleAsync(AllInOneVirtualAssistantCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            AllInOneVirtualAssistantMessageBody message = command.MessageBody as AllInOneVirtualAssistantMessageBody;

            bool succeeded = await _handler.ExecuteMessageAsync(message);

            if (succeeded == true)
            {
                _logger.LogDebug($"{nameof(AllInOneVirtualAssistantCommand)} phase finished executing successfully");
            }
            else
            {
                _logger.LogDebug($"{nameof(AllInOneVirtualAssistantCommand)} phase finished executing unsuccessfully");
            }
        }
    }
}
