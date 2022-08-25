using Domain.Models.Requests;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface INetworkingServiceApi
    {
        Task<HttpResponseMessage> GetSearchUrlProgressAsync(GetSearchUrlProgressRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> ProcessContactedCampaignProspectListAsync(CampaignProspectListRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> UpdateSearchUrlAsync(UpdateSearchUrlProgressRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessProspectListAsync(CollectedProspectsRequest request, CancellationToken ct = default);
    }
}
