using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Executors.MonitorForNewConnections
{
    public class CheckOffHoursNewConnectionsExecutorHandler : IMessageExecutorHandler<CheckOffHoursNewConnectionsBody>
    {
        public CheckOffHoursNewConnectionsExecutorHandler(
            ILogger<CheckOffHoursNewConnectionsExecutorHandler> logger,
            ICheckOffHoursNewConnectionsPhaseOrchestrator phaseOrchestrator,
            IMonitorForNewConnectionsService service)
        {
            _logger = logger;
            _phaseOrchestrator = phaseOrchestrator;
            _service = service;
        }

        private readonly ILogger<CheckOffHoursNewConnectionsExecutorHandler> _logger;
        private readonly ICheckOffHoursNewConnectionsPhaseOrchestrator _phaseOrchestrator;
        private readonly IMonitorForNewConnectionsService _service;

        public async Task<bool> ExecuteMessageAsync(CheckOffHoursNewConnectionsBody message)
        {
            bool succeeded = false;
            try
            {
                _phaseOrchestrator.Execute(message);
                succeeded = true;
            }
            catch
            {
                succeeded = false;
            }
            finally
            {
                await ProcessRecentlyAddedProspects(message);
            }

            return succeeded;
        }

        private async Task ProcessRecentlyAddedProspects(CheckOffHoursNewConnectionsBody message)
        {
            IList<Models.RecentlyAddedProspect> recentlyAddedProspects = _phaseOrchestrator.RecentlyAddedProspects;
            if (recentlyAddedProspects.Count > 0)
            {
                await _service.ProcessRecentlyAddedProspectsAsync(recentlyAddedProspects, message);
            }
        }
    }
}
