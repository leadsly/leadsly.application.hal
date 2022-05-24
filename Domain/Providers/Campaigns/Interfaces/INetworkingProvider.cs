using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface INetworkingProvider
    {
        Task<HalOperationResult<T>> ExecuteProspectListAsync<T>(NetworkingMessageBody message, SearchUrlProgress searchUrlProgress, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecuteSendConnectionsAsync<T>(NetworkingMessageBody message)
            where T : IOperationResponse;
    }
}
