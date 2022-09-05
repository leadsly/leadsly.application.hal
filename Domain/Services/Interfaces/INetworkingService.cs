using Domain.Models.ProspectList;
using Domain.Models.RabbitMQMessages;
using Domain.Models.Responses;
using Domain.Models.SendConnections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface INetworkingService
    {
        Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(NetworkingMessageBody message, CancellationToken ct = default);
        Task ProcessSentConnectionsAsync(IList<ConnectionSent> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task UpdateSearchUrlsAsync(IList<Domain.Models.Networking.SearchUrlProgress> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task ProcessProspectListAsync(IList<PersistPrimaryProspect> items, NetworkingMessageBody message, CancellationToken ct = default);
        Task UpdateMonthlySearchLimitAsync(bool limitReached, NetworkingMessageBody message, CancellationToken ct = default);
    }
}
