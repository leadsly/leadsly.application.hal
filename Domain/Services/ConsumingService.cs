using Domain.PhaseConsumers.AllInOneVirtualAssistantHandler;
using Domain.PhaseConsumers.FollowUpMessageHandlers;
using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingHandler;
using Domain.PhaseConsumers.RestartApplicationHandler;
using Domain.PhaseConsumers.ScanProspectsForRepliesHandlers;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
                /// Consume RestartApplication messages
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<RestartApplicationConsumerCommand> restartAppHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<RestartApplicationConsumerCommand>>();
                RestartApplicationConsumerCommand restartAppCommand = new RestartApplicationConsumerCommand(halIdentity.Id);
                await restartAppHandler.ConsumeAsync(restartAppCommand);

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
                /// Consume Networking [ ProspectListPhase AND SendConnectionsPhase ]
                ////////////////////////////////////////////////////////////////////////////////////
                HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand> networkingHandler = scope.ServiceProvider.GetRequiredService<HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand>>();
                NetworkingConsumerCommand networkingCommand = new NetworkingConsumerCommand(halIdentity.Id);
                await networkingHandler.ConsumeAsync(networkingCommand);
            }
        }

        public async Task StartConsumingAsync_AllInOneVirtualAssistant()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IHalIdentity halIdentity = scope.ServiceProvider.GetRequiredService<IHalIdentity>();

                ////////////////////////////////////////////////////////////////////////////////////
                /// Consume AllInOneVirtualAssistant messages
                ////////////////////////////////////////////////////////////////////////////////////
                IConsumeCommandHandler<AllInOneVirtualAssistantConsumerCommand> handler = scope.ServiceProvider.GetRequiredService<IConsumeCommandHandler<AllInOneVirtualAssistantConsumerCommand>>();
                AllInOneVirtualAssistantConsumerCommand allInOneCommand = new AllInOneVirtualAssistantConsumerCommand(halIdentity.Id);
                await handler.ConsumeAsync(allInOneCommand);
            }
        }
    }
}
