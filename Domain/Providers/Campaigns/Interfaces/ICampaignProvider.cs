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
        Task<HalOperationResult<T>> UpdateSendConnectionsUrlStatusesAsync<T>(IList<SearchUrlDetailsRequest> updatedSearchUrlsStatuses, SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> GetLatestSendConnectionsUrlStatusesAsync<T>(SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> MarkCampaignExhaustedAsync<T>(SendConnectionsBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> GetSearchUrlProgressAsync<T>(NetworkingMessageBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSearchUrlProgressAsync<T>(SearchUrlProgress updatedSearchUrlProgress, NetworkingMessageBody message, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
