using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface IScanProspectsForRepliesProvider
    {
        Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ScanProspectsForRepliesBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhaseOnce<T>(ScanProspectsForRepliesBody message)
            where T : IOperationResponse;
    }
}
