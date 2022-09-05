using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.RestartApplicationHandler
{
    public class RestartApplicationCommandHandler : ICommandHandler<RestartApplicationCommand>
    {
        public RestartApplicationCommandHandler(
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<RestartApplicationCommandHandler> logger)
        {
            _logger = logger;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        private readonly ILogger<RestartApplicationCommandHandler> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public Task HandleAsync(RestartApplicationCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;
            channel.BasicAck(args.DeliveryTag, false);

            _hostApplicationLifetime.StopApplication();

            return Task.CompletedTask;
        }
    }
}
