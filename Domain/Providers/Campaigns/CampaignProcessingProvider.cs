using Domain.Providers.Campaigns.Interfaces;
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
    public class CampaignProcessingProvider : ICampaignProcessingProvider
    {
        public CampaignProcessingProvider(ICampaignPhaseProcessingService campaignPhaseProcessingService, ILogger<CampaignProcessingProvider> logger)
        {
            _campaignPhaseProcessingService = campaignPhaseProcessingService;
        }

        private ICampaignPhaseProcessingService _campaignPhaseProcessingService;
        private ILogger<CampaignProcessingProvider> _logger;

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

        public async Task<HalOperationResult<T>> PerssistCampaingProspectsAsync<T>(IOperationResponse resultValue, SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // cast resultValue to ProspectList
            ICampaignProspectListPayload campaignProspectList = resultValue as ICampaignProspectListPayload;
            if (campaignProspectList == null)
            {
                _logger.LogError("Failed to cast resultValue into ICampaignProspectListPayload");
                return result;
            }

            CampaignProspectListRequest request = new()
            {
                HalId = message.HalId,
                UserId = message.UserId,
                CampaignId = message.CampaignId,
                CampaignProspects = campaignProspectList.CampaignProspects,
                RequestUrl = "api/campaign/prospects",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _campaignPhaseProcessingService.ProcessCampaignProspectListAsync(request, ct);
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

        public async Task<HalOperationResult<T>> TriggerSendConnectionsPhaseAsync<T>(string campaignId, string userId, SendConnectionsBody message, CancellationToken ct = default) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            TriggerSendConnectionsRequest request = new()
            {
                CampaignId = campaignId,
                UserId = userId,
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
    }
}
