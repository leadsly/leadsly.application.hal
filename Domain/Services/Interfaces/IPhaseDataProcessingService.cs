using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IPhaseDataProcessingService
    {
        Task<HttpResponseMessage> ProcessNewConnectionsAsync(NewProspectConnectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessProspectListAsync(Leadsly.Application.Model.Requests.FromHal.CollectedProspectsRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> MarkProspectListCompleteAsync(MarkProspectListPhaseCompleteRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessContactedCampaignProspectListAsync(Leadsly.Application.Model.Requests.FromHal.CampaignProspectListRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessNewlyAcceptedProspectsAsync(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default);

        /// <summary>
        /// Executed by ScanProspectsForRepliesPhase and DeepScanProspectsForRepliesPhase.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> ProcessProspectsRepliedAsync(ProspectsRepliedRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> ProcessFollowUpMessageSentAsync(FollowUpMessageSentRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> UpdateSocialAccountMonthlySearchLimitAsync(UpdateSocialAccountRequest request, CancellationToken ct = default);
    }
}
