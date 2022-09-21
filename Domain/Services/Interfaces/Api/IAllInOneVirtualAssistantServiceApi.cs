using Domain.Models.Requests.MonitorForNewConnections;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface IAllInOneVirtualAssistantServiceApi
    {
        Task<HttpResponseMessage> GetAllPreviouslyConnectedNetworkProspectsAsync(PreviouslyConnectedNetworkProspectsRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> UpdatePreviouslyConnectedNetworkProspectsAsync(UpdateConnectedNetworkProspectsRequest request, CancellationToken ct = default);
    }
}
