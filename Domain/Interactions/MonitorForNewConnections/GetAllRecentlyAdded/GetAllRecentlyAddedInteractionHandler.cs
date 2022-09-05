using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded
{
    public class GetAllRecentlyAddedInteractionHandler : IGetAllRecentlyAddedInteractionHandler
    {
        public GetAllRecentlyAddedInteractionHandler(ILogger<GetAllRecentlyAddedInteractionHandler> logger, IMonitorForNewConnectionsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetAllRecentlyAddedInteractionHandler> _logger;
        private readonly IMonitorForNewConnectionsServicePOM _service;
        private IList<Models.RecentlyAddedProspect> RecentlyAddedProspects { get; set; } = new List<Models.RecentlyAddedProspect>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetAllRecentlyAddedInteraction getAllRecentlyInteraction = interaction as GetAllRecentlyAddedInteraction;
            IList<Models.RecentlyAddedProspect> recentlyAdded = _service.GetAllRecentlyAdded(getAllRecentlyInteraction.WebDriver);
            if (recentlyAdded == null)
            {
                // handle failures or retries here
                return false;
            }

            RecentlyAddedProspects = recentlyAdded;
            return true;
        }

        public IList<Models.RecentlyAddedProspect> GetRecentlyAddedProspects()
        {
            IList<Models.RecentlyAddedProspect> prospects = RecentlyAddedProspects;
            RecentlyAddedProspects = new List<Models.RecentlyAddedProspect>();
            return prospects;
        }
    }
}
