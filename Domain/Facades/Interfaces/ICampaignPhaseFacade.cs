using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades.Interfaces
{
    public interface ICampaignPhaseFacade
    {
        Task<HalOperationResult<T>> ExecutePhaseAsync<T>(FollowUpMessageBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ScanProspectsForRepliesBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecuteDeepScanPhaseAsync<T>(ScanProspectsForRepliesBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecuteOffHoursScanPhaseAsync<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ProspectListBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhaseAsync<T>(SendConnectionsBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhase<T>(ConnectionWithdrawBody message)
            where T : IOperationResponse;

    }
}
