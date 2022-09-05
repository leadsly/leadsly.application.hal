using Domain.Models.Requests;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface IMonitorProspectsForNewConnectionsServiceApi
    {
        Task<HttpResponseMessage> ProcessNewlyDetectedProspectsAsync(RecentlyAddedProspectsRequest request, CancellationToken ct = default);
    }
}
