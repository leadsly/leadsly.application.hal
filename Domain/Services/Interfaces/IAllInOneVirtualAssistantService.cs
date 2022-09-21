using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.Responses;
using Domain.Models.ScanProspectsForReplies;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IAllInOneVirtualAssistantService
    {
        Task<PreviouslyConnectedNetworkProspectsResponse> GetAllPreviouslyConnectedNetworkProspectsAsync(PublishMessageBody message, CancellationToken ct = default);
        Task UpdatePreviouslyConnectedNetworkProspectsAsync(PublishMessageBody message, IList<RecentlyAddedProspectModel> items, int previousTotalConnectionsCount, CancellationToken ct = default);
        Task ProcessRecentlyAddedProspectsAsync(IList<RecentlyAddedProspectModel> requests, PublishMessageBody message, CancellationToken ct = default);
        Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageModel item, PublishMessageBody message, CancellationToken ct = default);
        Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(PublishMessageBody message, CancellationToken ct = default);
        Task ProcessSentConnectionsAsync(IList<ConnectionSentModel> items, PublishMessageBody message, CancellationToken ct = default);
        Task UpdateSearchUrlsAsync(IList<SearchUrlProgressModel> items, PublishMessageBody message, CancellationToken ct = default);
        Task ProcessProspectListAsync(IList<PersistPrimaryProspectModel> items, PublishMessageBody message, CancellationToken ct = default);
        Task UpdateMonthlySearchLimitAsync(bool limitReached, PublishMessageBody message, CancellationToken ct = default);
        Task ProcessNewMessagesAsync(IList<NewMessageModel> items, PublishMessageBody message, CancellationToken ct = default);
    }
}
