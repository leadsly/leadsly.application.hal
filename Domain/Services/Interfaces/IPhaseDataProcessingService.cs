using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IPhaseDataProcessingService
    {
        Task<HttpResponseMessage> ProcessNewConnectionsAsync(NewProspectConnectionRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessProspectListAsync(ProspectListPhaseCompleteRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> ProcessContactedCampaignProspectListAsync(CampaignProspectListRequest request, CancellationToken ct = default);        
        Task<HttpResponseMessage> ProcessNewlyAcceptedProspectsAsync(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default);

        /// <summary>
        /// Executed by ScanProspectsForRepliesPhase and DeepScanProspectsForRepliesPhase.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> ProcessProspectsRepliedAsync(ProspectsRepliedRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> ProcessFollowUpMessageSentAsync(FollowUpMessageSentRequest request, CancellationToken ct = default);
    }
}
