using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface IPhaseDataProcessingProvider
    {
        Task<HalOperationResult<T>> ProcessProspectListAsync<T>(IList<PrimaryProspectRequest> collectedProspects, PublishMessageBody message, string campaignId, string primaryProspectListId, string campaignProspectListId, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> MarkProspectListPhaseCompleteAsync<T>(ProspectListBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSocialAccountMonthlySearchLimitAsync<T>(string socialAccountId, PublishMessageBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(IList<CampaignProspectRequest> campaignProspects, PublishMessageBody message, string campaignId, CancellationToken ct = default)
            where T : IOperationResponse;

        /// <summary>
        /// Executed by DeepScanProspectsForRepliesPhase. We have a pre-defined list of prospects who to perform deep scan on.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prospectsReplied"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HalOperationResult<T>> ProcessProspectsThatRepliedAsync<T>(IList<ProspectRepliedRequest> prospectsReplied, ScanProspectsForRepliesBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        /// <summary>
        /// Executed by ScanProspectsForRepliesPhase. Because we don't know if the new message comes from one of our campaign's prospects or not
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prospectsReplied"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(IList<ProspectRepliedRequest> prospectsReplied, ScanProspectsForRepliesBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessSentFollowUpMessageAsync<T>(FollowUpMessageSentRequest sentFollowUpMessageRequest, FollowUpMessageBody message, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
