using Leadsly.Application.Model.Requests.FromHal;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ITriggerPhaseService
    {
        Task<HttpResponseMessage> TriggerCampaignProspectListAsync(TriggerSendConnectionsRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> TriggerScanProspectsForRepliesAsync(TriggerScanProspectsForRepliesRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> TriggerFollowUpMessageAsync(TriggerFollowUpMessageRequest request, CancellationToken ct = default);
    }
}
