using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.interfaces;
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
    public class PhaseDataProcessingProvider : IPhaseDataProcessingProvider
    {
        public PhaseDataProcessingProvider(
            ILogger<PhaseDataProcessingProvider> logger,
            IPhaseDataProcessingService phaseDataProcessingService)
        {
            _phaseDataProcessingService = phaseDataProcessingService;
            _logger = logger;
        }

        private readonly IPhaseDataProcessingService _phaseDataProcessingService;
        private readonly ILogger<PhaseDataProcessingProvider> _logger;

        public async Task<HalOperationResult<T>> ProcessProspectListAsync<T>(IOperationResponse resultValue, ProspectListBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // cast resultValue to ProspectList
            IPrimaryProspectListPayload primaryProspectList = resultValue as IPrimaryProspectListPayload;
            if (primaryProspectList == null)
            {
                _logger.LogError("Failed to cast resultValue into IPrimaryProspectList");
                return result;
            }

            ProspectListPhaseCompleteRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = message.CampaignId,
                PrimaryProspectListId = message.PrimaryProspectListId,
                CampaignProspectListId = message.CampaignProspectListId,
                Prospects = primaryProspectList.Prospects,
                RequestUrl = $"api/ProspectList/{message.HalId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.ProcessProspectListAsync(request, ct);
            if (responseMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for saving primary prospects to the database");
                return result;
            }

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(IList<CampaignProspectRequest> campaignProspects, SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            CampaignProspectListRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = message.CampaignId,
                CampaignProspects = campaignProspects,
                RequestUrl = $"api/SendConnections/{message.CampaignId}/prospects",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.ProcessContactedCampaignProspectListAsync(request, ct);
            if (responseMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for saving primary prospects to the database");
                return result;
            }

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        /// <summary>
        /// Processes those prospects that have replied to our message after running DeepScanProspectsForReplies phase.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prospectsReplied"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<HalOperationResult<T>> ProcessProspectsThatRepliedAsync<T>(IList<ProspectRepliedRequest> prospectsReplied, ScanProspectsForRepliesBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            ProspectsRepliedRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/DeepScanProspectsForReplies/{message.HalId}",
                ProspectsReplied = prospectsReplied
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.ProcessProspectsRepliedAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating campaign prospects who have responded to our messages and recording their response");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(IList<ProspectRepliedRequest> prospectsReplied, ScanProspectsForRepliesBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            ProspectsRepliedRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/ScanProspectsForReplies/{message.HalId}/prospects-replies",
                ProspectsReplied = prospectsReplied
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.ProcessProspectsRepliedAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating campaign prospects who have responded to our messages and recording their response");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessSentFollowUpMessageAsync<T>(FollowUpMessageSentRequest sentFollowUpMessageRequest, FollowUpMessageBody message, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            sentFollowUpMessageRequest.NamespaceName = message.NamespaceName;
            sentFollowUpMessageRequest.ServiceDiscoveryName = message.ServiceDiscoveryName;
            sentFollowUpMessageRequest.RequestUrl = $"api/FollowUpMessage/{sentFollowUpMessageRequest.CampaignProspectId}/follow-up";

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.ProcessFollowUpMessageSentAsync(sentFollowUpMessageRequest, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating campaign prospects who were delivered a follow up message");
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
