using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CampaignProspectListRequest = Leadsly.Application.Model.Requests.FromHal.CampaignProspectListRequest;
using CollectedProspectsRequest = Leadsly.Application.Model.Requests.FromHal.CollectedProspectsRequest;

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

        public async Task<HalOperationResult<T>> MarkProspectListPhaseCompleteAsync<T>(ProspectListBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            MarkProspectListPhaseCompleteRequest request = new()
            {
                RequestUrl = $"ProspectListPhase/{message.ProspectListPhaseId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                HalId = message.HalId
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.MarkProspectListCompleteAsync(request, ct);

            string prospectListPhaseId = message.ProspectListPhaseId;
            if (responseMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for marking prospect list phase {prospectListPhaseId} as complete", prospectListPhaseId);
                return result;
            }

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for marking prospect list phase {prospectListPhaseId} as complete", prospectListPhaseId);
                return result;
            }

            result.Succeeded = true;
            return result;

        }

        public async Task<HalOperationResult<T>> ProcessProspectListAsync<T>(
            IList<PrimaryProspectRequest> collectedProspects,
            PublishMessageBody message,
            string campaignId,
            string primaryProspectListId,
            string campaignProspectListId,
            CancellationToken ct = default
            ) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            CollectedProspectsRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = campaignId,
                PrimaryProspectListId = primaryProspectListId,
                CampaignProspectListId = campaignProspectListId,
                Prospects = collectedProspects,
                RequestUrl = $"ProspectList/{message.HalId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName
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

        public async Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(
                IList<CampaignProspectRequest> campaignProspects,
                PublishMessageBody message,
                string campaignId,
                CancellationToken ct = default
            ) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            CampaignProspectListRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = campaignId,
                CampaignProspects = campaignProspects,
                RequestUrl = $"SendConnections/{campaignId}/prospects",
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

            Leadsly.Application.Model.Requests.FromHal.ProspectsRepliedRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"DeepScanProspectsForReplies/{message.HalId}",
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

            Leadsly.Application.Model.Requests.FromHal.ProspectsRepliedRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"ScanProspectsForReplies/{message.HalId}/prospects-replied",
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

        public async Task<HalOperationResult<T>> UpdateSocialAccountMonthlySearchLimitAsync<T>(string socialAccountId, PublishMessageBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            UpdateSocialAccountRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"SocialAccounts/{socialAccountId}"
            };

            HttpResponseMessage responseMessage = await _phaseDataProcessingService.UpdateSocialAccountMonthlySearchLimitAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating social account 'MonthlySearchLimitReached' property");
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
            sentFollowUpMessageRequest.RequestUrl = $"FollowUpMessage/{sentFollowUpMessageRequest.CampaignProspectId}/follow-up";

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
