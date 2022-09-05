using Domain.Models.Requests;
using Domain.Models.Requests.Networking;
using Domain.Models.Requests.ProspectList;
using Domain.Models.Requests.SendConnections;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface INetworkingServiceApi
    {
        Task<HttpResponseMessage> GetSearchUrlProgressAsync(GetSearchUrlProgressRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> ProcessSentConnectionsAsync(ConnectionsSentRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> UpdateSearchUrlAsync(UpdateSearchUrlProgressRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessProspectListAsync(CollectedProspectsRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> UpdateMonthlySearchLimit(MonthlySearchLimitReachedRequest request, CancellationToken ct = default);
    }
}
