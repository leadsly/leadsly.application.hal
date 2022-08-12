using Domain.Providers.Campaigns.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class CampaignProvider : ICampaignProvider
    {
        public CampaignProvider(
            ICampaignService campaignService,
            ILogger<CampaignProvider> logger,
            ICampaignSerializer campaignSerializer)
        {
            _logger = logger;
            _campaignService = campaignService;
            _campaignSerializer = campaignSerializer;
        }

        private readonly ILogger<CampaignProvider> _logger;
        private readonly ICampaignService _campaignService;
        private readonly ICampaignSerializer _campaignSerializer;

        public async Task<HalOperationResult<T>> GetLatestSendConnectionsUrlStatusesAsync<T>(SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            SearchUrlDetailsRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"SendConnections/{message.CampaignId}/url"
            };

            HttpResponseMessage responseMessage = await _campaignService.GetLatestSentConnectionsUrlStatusesAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for getting latest sent connections url status");
                return result;
            }

            string json = await responseMessage.Content.ReadAsStringAsync();
            IGetSentConnectionsUrlStatusPayload sentConnectionStatuses = _campaignSerializer.DeserializeSentConnectionsUrlStatuses(json);
            if (sentConnectionStatuses == null)
            {
                return result;
            }

            result.Value = (T)sentConnectionStatuses;
            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> UpdateSendConnectionsUrlStatusesAsync<T>(IList<SearchUrlDetailsRequest> updatedSearchUrlsStatuses, SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            UpdateSearchUrlDetailsRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"SendConnections/{message.CampaignId}/url",
                SearchUrlDetailsRequests = updatedSearchUrlsStatuses
            };

            HttpResponseMessage responseMessage = await _campaignService.UpdateSendConnectionsUrlStatusesAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating sent connections url statuses");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> MarkCampaignExhaustedAsync<T>(SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            MarkCampaignExhaustedRequest request = new()
            {
                RequestUrl = $"campaigns/{message.CampaignId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                HalId = message.HalId
            };

            HttpResponseMessage responseMessage = await _campaignService.MarkCampaignExhausted(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating campaign");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> GetSearchUrlProgressAsync<T>(NetworkingMessageBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            SearchUrlProgressRequest request = new()
            {
                RequestUrl = $"Networking/{message.CampaignId}/url",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                HalId = message.HalId
            };

            HttpResponseMessage responseMessage = await _campaignService.GetSearchUrlProgressAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for getting search url progress");
                return result;
            }

            string json = await responseMessage.Content.ReadAsStringAsync();
            ISearchUrlProgressPayload sentConnectionStatuses = _campaignSerializer.DeserializeSearchUrlsProgress(json);
            if (sentConnectionStatuses == null)
            {
                return result;
            }

            result.Value = (T)sentConnectionStatuses;

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> UpdateSearchUrlProgressAsync<T>(SearchUrlProgressRequest updatedSearchUrlProgress, NetworkingMessageBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            UpdateSearchUrlProgressRequest request = new()
            {
                RequestUrl = $"Networking/{updatedSearchUrlProgress.SearchUrlProgressId}/url",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                HalId = message.HalId,
                LastPage = updatedSearchUrlProgress.LastPage,
                WindowHandleId = updatedSearchUrlProgress.WindowHandleId,
                SearchUrl = updatedSearchUrlProgress.SearchUrl,
                TotalSearchResults = updatedSearchUrlProgress.TotalSearchResults,
                SearchUrlProgressId = updatedSearchUrlProgress.SearchUrlProgressId,
                LastProcessedProspect = updatedSearchUrlProgress.LastProcessedProspect,
                LastActivityTimestamp = updatedSearchUrlProgress.LastActivityTimestamp,
                StartedCrawling = updatedSearchUrlProgress.StartedCrawling,
                Exhausted = updatedSearchUrlProgress.Exhausted
            };

            HttpResponseMessage responseMessage = await _campaignService.UpdateSearchUrlProgressAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating SearchUrlProgress");
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
