using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Models.RabbitMQMessages;
using Domain.Orchestrators;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Domain.Executors.MonitorForNewConnections
{
    public class MonitorForNewConnectionsExecutorHandler : IMessageExecutorHandler<MonitorForNewAcceptedConnectionsBody>
    {
        public MonitorForNewConnectionsExecutorHandler(
            ILogger<MonitorForNewConnectionsExecutorHandler> logger,
            IMonitorForNewConnectionsPhaseOrchestrator phaseOrchestrator,
            IMonitorForNewConnectionsService service
            )
        {
            _logger = logger;
            _phaseOrchestrator = phaseOrchestrator;
            _service = service;
        }

        private readonly IMonitorForNewConnectionsService _service;
        private readonly ILogger<MonitorForNewConnectionsExecutorHandler> _logger;
        private readonly IMonitorForNewConnectionsPhaseOrchestrator _phaseOrchestrator;

        public Task<bool> ExecuteMessageAsync(MonitorForNewAcceptedConnectionsBody message)
        {
            if (MonitorForNewConnectionsPhaseOrchestrator.IsRunning == false)
            {
                _logger.LogInformation("MonitorForNewProspects phase is currently NOT running. Executing the phase until the end of work day");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // this is required because this task can run for 8 - 10 hours a day. The AppServer does not know IF this task/phase is already
                // running on Hal thus it will trigger messages blindly. Otherwise if we await this here, then none of the blindly triggered
                // messages make it here, thus clugg up the queue
                Task.Run(() =>
                {
                    _phaseOrchestrator.Execute(message);
                });

                _phaseOrchestrator.NewConnectionsDetected += OnNewConnectionsDetected;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return Task.Run(() => true);
        }

        private async Task OnNewConnectionsDetected(object sender, NewRecentlyAddedProspectsDetectedEventArgs e)
        {
            _logger.LogDebug("New prospects have been detected. Sending them to the server for processing");
            MonitorForNewAcceptedConnectionsBody message = e.Message as MonitorForNewAcceptedConnectionsBody;

            await _service.ProcessRecentlyAddedProspectsAsync(e.NewRecentlyAddedProspects, message);
        }
    }
}
