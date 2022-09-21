using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.Responses;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface INetworkingService
    {
        Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(NetworkingMessageBody message, CancellationToken ct = default);
        Task ProcessSentConnectionsAsync(IList<ConnectionSentModel> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task UpdateSearchUrlsAsync(IList<SearchUrlProgressModel> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task ProcessProspectListAsync(IList<PersistPrimaryProspectModel> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task UpdateMonthlySearchLimitAsync(bool limitReached, NetworkingMessageBody message, CancellationToken ct = default);
    }
}
