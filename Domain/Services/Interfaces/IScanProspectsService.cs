using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IScanProspectsService
    {
        Task ProcessNewMessagesAsync(IList<NewMessageRequest> newMessageRequests, ScanProspectsForRepliesBody message, CancellationToken ct = default);
    }
}
