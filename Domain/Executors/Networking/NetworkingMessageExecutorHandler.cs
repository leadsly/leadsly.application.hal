using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Domain.Executors.Networking
{
    public class NetworkingMessageExecutorHandler : IMessageExecutorHandler<NetworkingMessageBody>
    {
        public NetworkingMessageExecutorHandler(
            ILogger<NetworkingMessageExecutorHandler> logger,
            INetworkingService service,
            INetworkingPhaseOrchestrator orchestrator
            )
        {
            _orchestrator = orchestrator;
            _logger = logger;
            _service = service;
        }

        private readonly INetworkingPhaseOrchestrator _orchestrator;
        private readonly INetworkingService _service;
        private readonly ILogger<NetworkingMessageExecutorHandler> _logger;

        public async Task<bool> ExecuteMessageAsync(NetworkingMessageBody message)
        {
            bool succeeded = false;
            GetSearchUrlProgressResponse response = await _service.GetSearchUrlProgressAsync(message);
            if (response == null || response.Items == null)
            {
                return succeeded;
            }

            if (response.Items.Count == 0)
            {
                _logger.LogInformation("There are no search urls left to crawl. All of search urls have been crawled.");
                return true;
            }

            try
            {
                _orchestrator.PersistPrimaryProspects += OnProcessProspectListAsync;
                _orchestrator.ConnectionsSent += OnProcessSentConnectionsAsync;
                _orchestrator.SearchLimitReached += OnUpdateMonthlySearchLimitAsync;
                _orchestrator.UpdatedSearchUrlsProgress += OnUpdateSearchUrlsAsync;

                _orchestrator.Execute(message, response.Items);
                succeeded = true;
            }
            catch (Exception ex)
            {
                succeeded = false;
            }

            return succeeded;
        }

        private async Task OnProcessSentConnectionsAsync(object sender, ConnectionsSentEventArgs e)
        {
            if (e.ConnectionsSent?.Count > 0)
            {
                NetworkingMessageBody networkingMessage = e.Message as NetworkingMessageBody;
                await _service.ProcessSentConnectionsAsync(e.ConnectionsSent, networkingMessage);
            }
        }

        private async Task OnUpdateSearchUrlsAsync(object sender, UpdatedSearchUrlProgressEventArgs e)
        {
            if (e.UpdatedSearchUrlsProgress?.Count > 0)
            {
                NetworkingMessageBody networkingMessage = e.Message as NetworkingMessageBody;
                await _service.UpdateSearchUrlsAsync(e.UpdatedSearchUrlsProgress, networkingMessage);
            }
        }

        private async Task OnProcessProspectListAsync(object sender, PersistPrimaryProspectsEventArgs e)
        {
            if (e.PersistPrimaryProspects?.Count > 0)
            {
                NetworkingMessageBody networkingMessage = e.Message as NetworkingMessageBody;
                await _service.ProcessProspectListAsync(e.PersistPrimaryProspects, networkingMessage);
            }
        }

        private async Task OnUpdateMonthlySearchLimitAsync(object sender, MonthlySearchLimitReachedEventArgs e)
        {
            NetworkingMessageBody networkingMessage = e.Message as NetworkingMessageBody;
            await _service.UpdateMonthlySearchLimitAsync(e.LimitReached, networkingMessage);
        }

    }
}
