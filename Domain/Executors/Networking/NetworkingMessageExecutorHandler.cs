using Domain.Models.ProspectList;
using Domain.Models.Responses;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Executors.Networking
{
    public class NetworkingMessageExecutorHandler : IMessageExecutorHandler<NetworkingMessageBody>
    {
        public NetworkingMessageExecutorHandler(
            ILogger<NetworkingMessageExecutorHandler> logger,
            INetworkingService networkingService,
            INetworkingPhaseOrchestrator phaseOrchestrator
            )
        {
            _phaseOrchestrator = phaseOrchestrator;
            _logger = logger;
            _networkingService = networkingService;
        }

        private readonly INetworkingPhaseOrchestrator _phaseOrchestrator;
        private readonly INetworkingService _networkingService;
        private readonly ILogger<NetworkingMessageExecutorHandler> _logger;

        public async Task<bool> ExecuteMessageAsync(NetworkingMessageBody message)
        {
            bool succeeded = false;
            GetSearchUrlProgressResponse response = await _networkingService.GetSearchUrlProgressAsync(message);
            if (response == null || response.SearchUrls == null)
            {
                return succeeded;
            }

            if (response.SearchUrls.Count == 0)
            {
                _logger.LogInformation("There are no search urls left to crawl. All of search urls have been crawled.");
                return true;
            }

            try
            {
                _phaseOrchestrator.Execute(message, response.SearchUrls);
                succeeded = true;
            }
            catch (Exception ex)
            {
                succeeded = false;
            }
            finally
            {
                await ProcessDataAsync(message);
            }

            return succeeded;
        }

        private async Task ProcessDataAsync(NetworkingMessageBody message)
        {
            // perform all async calls now.
            IList<ConnectionSentModel> connectionSents = _phaseOrchestrator.ConnectionsSent;
            if (connectionSents.Count > 0)
            {
                await _networkingService.ProcessSentConnectionsAsync(connectionSents, message);
            }

            IList<Domain.Models.Networking.SearchUrlProgressModel> items = _phaseOrchestrator.GetUpdatedSearchUrlsProgress();
            if (items.Count > 0)
            {
                await _networkingService.UpdateSearchUrlsAsync(items, message);
            }

            List<PersistPrimaryProspectModel> persistPrimaryProspects = _phaseOrchestrator.PersistPrimaryProspects;
            if (persistPrimaryProspects.Count > 0)
            {
                await _networkingService.ProcessProspectListAsync(persistPrimaryProspects, message);
            }

            bool monthlyLimitReached = _phaseOrchestrator.GetMonthlySearchLimitReached();
            await _networkingService.UpdateMonthlySearchLimitAsync(monthlyLimitReached, message);

        }
    }
}
