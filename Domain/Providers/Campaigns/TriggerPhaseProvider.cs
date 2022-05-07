using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class TriggerPhaseProvider : ITriggerPhaseProvider
    {
        public TriggerPhaseProvider(ITriggerPhaseService triggerPhaseService, ILogger<TriggerPhaseProvider> logger)
        {
            _triggerPhaseService = triggerPhaseService;
            _logger = logger;
        }

        private ILogger<TriggerPhaseProvider> _logger;
        private ITriggerPhaseService _triggerPhaseService;

        public async Task<HalOperationResult<T>> TriggerSendConnectionsPhaseAsync<T>(ProspectListBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            TriggerSendConnectionsRequest request = new()
            {
                CampaignId = message.CampaignId,
                UserId = message.UserId,
                HalId = message.HalId,
                RequestUrl = $"api/SendConnections/{message.HalId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _triggerPhaseService.TriggerCampaignProspectListAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> TriggerScanProspectsForRepliesPhaseAsync<T>(ScanProspectsForRepliesBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            TriggerScanProspectsForRepliesRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/ScanProspectsForReplies/{message.HalId}",
                UserId = message.UserId
            };

            HttpResponseMessage responseMessage = await _triggerPhaseService.TriggerScanProspectsForRepliesAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                string halId = message.HalId;
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for triggering ScanProspectsForRepliesPhase for hal id {halId}", halId);
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> TriggerFollowUpMessagesPhaseAsync<T>(ScanProspectsForRepliesBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            TriggerFollowUpMessageRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/FollowUpMessage/{message.HalId}",
                UserId = message.UserId
            };

            HttpResponseMessage responseMessage = await _triggerPhaseService.TriggerFollowUpMessageAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                string halId = message.HalId;
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for triggering FollowUpMessagePhase for hal id {halId}", halId);
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
