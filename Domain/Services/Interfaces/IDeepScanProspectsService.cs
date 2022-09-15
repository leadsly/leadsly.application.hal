using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IDeepScanProspectsService
    {
        Task<NetworkProspectsResponse> GetAllProspectsFromActiveCampaignsAsync(DeepScanProspectsForRepliesBody message, CancellationToken ct = default);
        Task ProcessCampaignProspectsThatRepliedAsync(IList<ProspectRepliedModel> prospects, DeepScanProspectsForRepliesBody message, CancellationToken ct = default);

    }
}
