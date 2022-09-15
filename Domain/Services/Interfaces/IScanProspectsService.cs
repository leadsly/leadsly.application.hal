using Domain.Models.ScanProspectsForReplies;
using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IScanProspectsService
    {
        Task ProcessNewMessagesAsync(IList<NewMessageModel> items, ScanProspectsForRepliesBody message, CancellationToken ct = default);
    }
}
