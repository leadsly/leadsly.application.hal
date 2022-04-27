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
    public interface IMonitorForNewProspectsProvider
    {
        Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ExecutePhaseOffHoursScanPhaseAsync<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse;

    }
}
