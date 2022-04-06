using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface ICampaignProvider
    {
        Task<HalOperationResult<T>> PersistProspectListAsync<T>(IOperationResponse resultValue, ProspectListBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(IList<CampaignProspectRequest> campaignProspects, SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> TriggerSendConnectionsPhaseAsync<T>(ProspectListBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSendConnectionsUrlStatusesAsync<T>(IList<SentConnectionsUrlStatusRequest> updatedSearchUrlsStatuses, SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> GetLatestSendConnectionsUrlStatusesAsync<T>(SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
