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
        public CampaignProvider(ICampaignPhaseProcessingService campaignPhaseProcessingService, ICampaignService campaignService, ILogger<CampaignProvider> logger, ICampaignSerializer campaignSerializer)
        {
            _campaignPhaseProcessingService = campaignPhaseProcessingService;
            _campaignService = campaignService;
            _campaignSerializer = campaignSerializer;
        }

        private ICampaignPhaseProcessingService _campaignPhaseProcessingService;
        private ILogger<CampaignProvider> _logger;
        private ICampaignService _campaignService;
        private ICampaignSerializer _campaignSerializer;

        public async Task<HalOperationResult<T>> PersistProspectListAsync<T>(IOperationResponse resultValue, ProspectListBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // cast resultValue to ProspectList
            IPrimaryProspectListPayload primaryProspectList = resultValue as IPrimaryProspectListPayload;
            if(primaryProspectList == null)
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
                Prospects = primaryProspectList.Prospects,
                RequestUrl = "api/prospect-list",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _campaignPhaseProcessingService.ProcessProspectListAsync(request, ct);
            if(responseMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for saving primary prospects to the database");
                return result;
            }

            if(responseMessage.IsSuccessStatusCode == false)
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

            // cast resultValue to ProspectList
            //ISendConnectionsPayload payload = resultValue as ISendConnectionsPayload;
            //if (payload == null)
            //{
            //    _logger.LogError("Failed to cast resultValue into ICampaignProspectListPayload");
            //    return result;
            //}

            CampaignProspectListRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = message.CampaignId,
                CampaignProspects = campaignProspects,
                RequestUrl = $"api/campaigns/{message.CampaignId}/prospects",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _campaignPhaseProcessingService.UpdateContactedCampaignProspectListAsync(request, ct);
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

        public async Task<HalOperationResult<T>> TriggerSendConnectionsPhaseAsync<T>(ProspectListBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            TriggerSendConnectionsRequest request = new()
            {
                CampaignId = message.CampaignId,
                UserId = message.UserId,
                HalId = message.HalId,
                RequestUrl = "api/campaignphases/trigger-send-connection-requests",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _campaignPhaseProcessingService.TriggerCampaignProspectListAsync(request, ct);

            if (responseMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

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
    }
}
