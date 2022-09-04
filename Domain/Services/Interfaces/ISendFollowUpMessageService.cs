using Domain.Models.Requests;
using Leadsly.Application.Model.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ISendFollowUpMessageService
    {
        Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageRequest request, FollowUpMessageBody message, CancellationToken ct = default);
    }
}
