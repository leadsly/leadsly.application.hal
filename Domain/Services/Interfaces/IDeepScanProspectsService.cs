using Domain.Models;
using Domain.Models.Responses;
using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IDeepScanProspectsService
    {
        Task<NetworkProspectsResponse> GetAllProspectsFromActiveCampaignsAsync(DeepScanProspectsForRepliesBody message, CancellationToken ct = default);
        Task ProcessCampaignProspectsThatRepliedAsync(IList<ProspectReplied> prospects, DeepScanProspectsForRepliesBody message, CancellationToken ct = default);

    }
}
