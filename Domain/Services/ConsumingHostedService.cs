using Domain.OptionsJsonModels;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ConsumingHostedService : IHostedService
    {
        public ConsumingHostedService(IConsumingService consumingService, IOptions<FeatureFlagsOptions> options)
        {
            _consumingService = consumingService;
            _options = options.Value;
        }

        private readonly FeatureFlagsOptions _options;
        private readonly IConsumingService _consumingService;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.AllInOneVirtualAssistant == true)
            {
                await _consumingService.StartConsumingAsync_AllInOneVirtualAssistant();
            }
            else
            {
                await _consumingService.StartConsumingAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
