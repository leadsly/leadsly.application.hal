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
        HalOperationResult<T> ExecutePhase<T>(FollowUpMessagesBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhase<T>(ScanProspectsForRepliesBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhase<T>(ProspectListBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhase<T>(SendConnectionRequestsBody message)
            where T : IOperationResponse;

        HalOperationResult<T> ExecutePhase<T>(ConnectionWithdrawBody message)
            where T : IOperationResponse;

    }
}
