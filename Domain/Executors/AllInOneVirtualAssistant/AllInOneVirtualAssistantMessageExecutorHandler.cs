using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant
{
    public class AllInOneVirtualAssistantMessageExecutorHandler : IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody>
    {
        public AllInOneVirtualAssistantMessageExecutorHandler(
            ILogger<AllInOneVirtualAssistantMessageExecutorHandler> logger,
            IAllInOneVirtualAssistantPhaseOrchestrator orchestrator)
        {
            _logger = logger;
            _orchestrator = orchestrator;
        }

        private readonly ILogger<AllInOneVirtualAssistantMessageExecutorHandler> _logger;
        private readonly IAllInOneVirtualAssistantPhaseOrchestrator _orchestrator;

        public async Task<bool> ExecuteMessageAsync(AllInOneVirtualAssistantMessageBody message)
        {
            try
            {
                // wire up the event handler
                _orchestrator.NewConnectionsDetected += OnNewConnectionsDetected;

                // fetch previous connected with prospects, this list should include the total connections count, as well as a list of
                // prospects first name last name subheading and when we connected with them
                PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse = await _service.GetAllPreviouslyConnectedNetworkProspectsAsync(message);
                PreviouslyScannedForRepliesProspectsResponse previousScannedResponse = await _service.GetAllPreviouslyScannedForRepliesProspectsAsync(message);
                if (previousMonitoredResponse == null || previousScannedResponse)
                {
                    _logger.LogError("Error occured executing {0}. An error occured retrieving data. The response was null. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
                }
                else
                {
                    _orchestrator.Execute(message, previousMonitoredResponse, previousScannedResponse);
                }
            }
            finally
            {

            }
        }

        private async Task UpdatePreviouslyConnectedNetworkProspectsAsync(AllInOneVirtualAssistantMessageBody message)
        {
            _logger.LogInformation("Executing {0}. Preparing request to update previously connected network prospects. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            await _service.ProcessPreviouslyConnectedNetworkProspectsAsync(message, _orchestrator.PreviouslyConnectedNetworkProspects);
        }

        private async Task OnNewConnectionsDetected(object sender, NewRecentlyAddedProspectsDetectedEventArgs e)
        {
            _logger.LogDebug("Executing {0}. New prospects have been detected. Sending them to the server for processing", nameof(AllInOneVirtualAssistantMessageBody));
            AllInOneVirtualAssistantMessageBody message = e.Message as AllInOneVirtualAssistantMessageBody;

            await _service.ProcessRecentlyAddedProspectsAsync(e.NewRecentlyAddedProspects, message);
        }
    }
}
