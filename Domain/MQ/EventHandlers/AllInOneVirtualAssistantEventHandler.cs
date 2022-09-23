using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Messages;
using Domain.PhaseHandlers.AllInOneVirtualAssistantHandler;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers
{
    public class AllInOneVirtualAssistantEventHandler : RabbitMQEventHandlerBase, IAllInOneVirtualAssistantEventHandler
    {
        public AllInOneVirtualAssistantEventHandler(
            ILogger<AllInOneVirtualAssistantEventHandler> logger,
            ICommandHandler<AllInOneVirtualAssistantCommand> handler
            ) : base(logger)
        {
            _logger = logger;
            _handler = handler;
        }

        private readonly ILogger<AllInOneVirtualAssistantEventHandler> _logger;
        private readonly ICommandHandler<AllInOneVirtualAssistantCommand> _handler;

        public async Task OnAllInOneVirtualAssistantEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string rawMessage = Encoding.UTF8.GetString(body);
            PublishMessageBody followUpMessages = DeserializeMessage<AllInOneVirtualAssistantMessageBody>(rawMessage);

            AllInOneVirtualAssistantCommand command = new AllInOneVirtualAssistantCommand(channel, eventArgs, followUpMessages);
            await _handler.HandleAsync(command);
        }
    }
}
