using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces;
using Domain.Models.MonitorForNewProspects;
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
        private IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; set; } = new List<RecentlyAddedProspectModel>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetAllRecentlyAddedInteraction getAllRecentlyInteraction = interaction as GetAllRecentlyAddedInteraction;
            IList<RecentlyAddedProspectModel> recentlyAdded = _service.GetAllRecentlyAdded(getAllRecentlyInteraction.WebDriver);
            if (recentlyAdded == null)
            {
                // handle failures or retries here
                return false;
            }

            RecentlyAddedProspects = recentlyAdded;
            return true;
        }

        public IList<RecentlyAddedProspectModel> GetRecentlyAddedProspects()
        {
            IList<RecentlyAddedProspectModel> prospects = RecentlyAddedProspects;
            RecentlyAddedProspects = new List<RecentlyAddedProspectModel>();
            return prospects;
        }
    }
}
