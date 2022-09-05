using Domain.PhaseHandlers.RestartApplicationHandler;
using Domain.RabbitMQ.EventHandlers.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers
{
    public class RestartApplicationEventHandler : IRestartApplicationEventHandler
    {
        private readonly ILogger<RestartApplicationEventHandler> _logger;
        private readonly ICommandHandler<RestartApplicationCommand> _handler;

        public RestartApplicationEventHandler(
            ILogger<RestartApplicationEventHandler> logger,
            ICommandHandler<RestartApplicationCommand> handler)
        {
            _logger = logger;
            _handler = handler;
        }
        public async Task OnRestartApplicationEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;
            RestartApplicationCommand restartCommand = new RestartApplicationCommand(channel, eventArgs);
            await _handler.HandleAsync(restartCommand);
        }
    }
}
