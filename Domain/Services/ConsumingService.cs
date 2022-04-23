using Domain.PhaseConsumers;
using Domain.PhaseConsumers.FollowUpMessageHandlers;
using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingConnectionsHandlers;
using Domain.PhaseConsumers.ScanProspectsForRepliesHandlers;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ConsumingService : IConsumingService
    {
        public ConsumingService(ILogger<ConsumingService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private readonly ILogger<ConsumingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public async Task StartConsumingAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IHalIdentity halIdentity = scope.ServiceProvider.GetRequiredService<IHalIdentity>();

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume MonitorForNewConnections messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<MonitorForNewConnectionsConsumerCommand> monitorHandler = scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsConsumerCommand>>();
                MonitorForNewConnectionsConsumerCommand monitorCommand = new MonitorForNewConnectionsConsumerCommand(halIdentity.Id);
                await monitorHandler.HandleAsync(monitorCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume ScanProspectsForReplies messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand> scanProspectsHandler = scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand>>();
                ScanProspectsForRepliesConsumerCommand scanCommand = new ScanProspectsForRepliesConsumerCommand(halIdentity.Id);
                await scanProspectsHandler.HandleAsync(scanCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume FollowUpMessages messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<FollowUpMessageConsumerCommand> followUpHandler = scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<FollowUpMessageConsumerCommand>>();
                FollowUpMessageConsumerCommand followUpCommand = new FollowUpMessageConsumerCommand(halIdentity.Id);
                await followUpHandler.HandleAsync(followUpCommand);


                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume NetworkingConnections [ ProspectListPhase OR SendConnectionsPhase ] messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalWorkCommandHandlerDecorator<NetworkingConnectionsConsumerCommand> networkingHandler = scope.ServiceProvider.GetRequiredService<HalWorkCommandHandlerDecorator<NetworkingConnectionsConsumerCommand>>();
                NetworkingConnectionsConsumerCommand networkingCommand = new NetworkingConnectionsConsumerCommand(halIdentity.Id);
                await networkingHandler.HandleAsync(networkingCommand);
            }
        }
    }
}
