using Domain.Models.Requests.DeepScanProspectsForReplies;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface IDeepScanProspectsForRepliesServiceApi
    {
        Task<HttpResponseMessage> ProcessProspectsRepliedAsync(ProspectsRepliedRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> GetAllProspectsFromActiveCampaignsAsync(AllNetworkProspectsRequest request, CancellationToken ct = default);
    }
}
