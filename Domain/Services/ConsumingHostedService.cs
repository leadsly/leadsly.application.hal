using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Domain.Services.Interfaces;

namespace Domain.Services
{
    public class ConsumingHostedService : IHostedService
    {
        public ConsumingHostedService(IConsumingService consumingService)
        {
            _consumingService = consumingService;
        }

        private readonly IConsumingService _consumingService;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consumingService.StartConsuming();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
