using Domain.Models.RabbitMQMessages;
using Domain.Models.ScanProspectsForReplies;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IScanProspectsService
    {
        Task ProcessNewMessagesAsync(IList<NewMessage> items, ScanProspectsForRepliesBody message, CancellationToken ct = default);
    }
}
