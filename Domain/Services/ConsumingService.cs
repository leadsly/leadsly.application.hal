using Domain.PhaseConsumers;
using Domain.PhaseConsumers.FollowUpMessageHandlers;
using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingHandler;
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
                HalConsumingCommandHandlerDecorator<MonitorForNewConnectionsConsumerCommand> monitorHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<MonitorForNewConnectionsConsumerCommand>>();
                MonitorForNewConnectionsConsumerCommand monitorCommand = new MonitorForNewConnectionsConsumerCommand(halIdentity.Id);
                await monitorHandler.ConsumeAsync(monitorCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume ScanProspectsForReplies messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand> scanProspectsHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand>>();
                ScanProspectsForRepliesConsumerCommand scanCommand = new ScanProspectsForRepliesConsumerCommand(halIdentity.Id);
                await scanProspectsHandler.ConsumeAsync(scanCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume FollowUpMessages messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<FollowUpMessageConsumerCommand> followUpHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<FollowUpMessageConsumerCommand>>();
                FollowUpMessageConsumerCommand followUpCommand = new FollowUpMessageConsumerCommand(halIdentity.Id);
                await followUpHandler.ConsumeAsync(followUpCommand);


                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume NetworkingConnections [ ProspectListPhase OR SendConnectionsPhase ] messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<NetworkingConnectionsConsumerCommand> networkingConnectionsHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<NetworkingConnectionsConsumerCommand>>();
                NetworkingConnectionsConsumerCommand networkingConnectionCommand = new NetworkingConnectionsConsumerCommand(halIdentity.Id);
                await networkingConnectionsHandler.ConsumeAsync(networkingConnectionCommand);

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume Networking [ ProspectListPhase AND SendConnectionsPhase ]
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand> networkingHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand>>();
                NetworkingConsumerCommand networkingCommand = new NetworkingConsumerCommand(halIdentity.Id);
                await networkingHandler.ConsumeAsync(networkingCommand);

            }
        }
    }
}
