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
                Prospects = primaryProspectList.Prospects,
                RequestUrl = "api/prospect-list",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage responseMessage = await _campaignPhaseProcessingService.ProcessProspectListAsync(request, ct);
            if(responseMessage == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
