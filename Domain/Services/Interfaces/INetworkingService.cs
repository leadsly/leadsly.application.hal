using Domain.Models.Networking;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface INetworkingService
    {
        Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(NetworkingMessageBody message, CancellationToken ct = default);

        Task ProcessSentConnectionsAsync(IList<ConnectionSentRequest> requests, NetworkingMessageBody message, CancellationToken ct = default);

        Task UpdateSearchUrlsAsync(IList<UpdateSearchUrlProgressRequest> requests, NetworkingMessageBody message, CancellationToken ct = default);
        Task ProcessProspectListAsync(IList<PersistPrimaryProspectRequest> requests, NetworkingMessageBody message, CancellationToken ct = default);
    }
}
