using Domain.Models.FollowUpMessage;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IFollowUpMessageService
    {
        Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageModel item, FollowUpMessageBody message, CancellationToken ct = default);
        Task<FollowUpMessagesResponse> GetFollowUpMessagesAsync(PublishMessageBody message, CancellationToken ct = default);

    }
}
