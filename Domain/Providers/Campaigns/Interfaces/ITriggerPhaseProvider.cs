using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface ITriggerPhaseProvider
    {
        Task<HalOperationResult<T>> TriggerSendConnectionsPhaseAsync<T>(ProspectListBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> TriggerScanProspectsForRepliesPhaseAsync<T>(ScanProspectsForRepliesBody message, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> TriggerFollowUpMessagesPhaseAsync<T>(ScanProspectsForRepliesBody message, CancellationToken ct = default)
            where T : IOperationResponse;
    }
}
