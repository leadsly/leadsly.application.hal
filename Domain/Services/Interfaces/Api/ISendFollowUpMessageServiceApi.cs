using Domain.Models.Requests.FollowUpMessage;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface ISendFollowUpMessageServiceApi
    {
        Task<HttpResponseMessage> ProcessSentFollowUpMessageAsync(SentFollowUpMessageRequest request, CancellationToken ct = default);
        // Task<HttpResponseMessage> GetFollowUpMessagesAsync(GetFollowUpMessagesRequest request, CancellationToken ct = default);
    }
}
