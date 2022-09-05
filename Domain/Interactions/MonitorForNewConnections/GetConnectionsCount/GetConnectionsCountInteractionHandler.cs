using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.MonitorForNewConnections.GetConnectionsCount
{
    public class GetConnectionsCountInteractionHandler : IGetConnectionsCountInteractionHandler
    {
        public GetConnectionsCountInteractionHandler(ILogger<GetConnectionsCountInteractionHandler> logger, IMonitorForNewConnectionsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetConnectionsCountInteractionHandler> _logger;
        private readonly IMonitorForNewConnectionsServicePOM _service;
        private int ConnectionCount { get; set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetConnectionsCountInteraction getConnectionsInteraction = interaction as GetConnectionsCountInteraction;
            int? connectionCount = _service.GetConnectionsCount(getConnectionsInteraction.WebDriver);
            if (connectionCount == null)
            {
                // handle failures or retries here
                return false;
            }
            ConnectionCount = (int)connectionCount;
            return true;
        }

        public int GetConnectionsCount()
        {
            int connectionCount = ConnectionCount;
            ConnectionCount = 0;
            return connectionCount;
        }
    }
}
