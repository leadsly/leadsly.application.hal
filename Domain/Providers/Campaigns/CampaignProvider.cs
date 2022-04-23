using Domain.Providers.Campaigns.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.interfaces;
using Leadsly.Application.Model.Campaigns.Interfaces;
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

            SentConnectionsUrlStatusRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/campaigns/{message.CampaignId}/sent-connections-url-statuses"
            };

            HttpResponseMessage responseMessage = await _campaignService.GetLatestSentConnectionsUrlStatusesAsync(request, ct);

            if(responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for getting latest sent connections url status");
                return result;
            }

            string json = await responseMessage.Content.ReadAsStringAsync();
            IGetSentConnectionsUrlStatusPayload sentConnectionStatuses = _campaignSerializer.DeserializeSentConnectionsUrlStatuses(json);
            if(sentConnectionStatuses == null)
            {
                return result;
            }

            result.Value = (T)sentConnectionStatuses;
            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> UpdateSendConnectionsUrlStatusesAsync<T>(IList<SentConnectionsUrlStatusRequest> updatedSearchUrlsStatuses, SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            UpdateSentConnectionsUrlStatusRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"api/campaigns/{message.CampaignId}/sent-connections-url-statuses",
                SentConnectionsUrlStatuses = updatedSearchUrlsStatuses
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
                RequestUrl = $"api/campaigns/{message.CampaignId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                HalId = message.HalId
            };

            HttpResponseMessage responseMessage = await _campaignService.MarkCampaignExhausted(request, ct);

            if(responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating campaign");
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
