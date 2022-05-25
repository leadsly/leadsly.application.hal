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
        Task<HalOperationResult<T>> ExecuteNetworkingAsync<T>(NetworkingMessageBody message, IList<SearchUrlProgress> searchUrlsProgress, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
