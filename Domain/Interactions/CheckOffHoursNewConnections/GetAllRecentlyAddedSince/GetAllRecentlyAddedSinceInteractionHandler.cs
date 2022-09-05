using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince
{
    public class GetAllRecentlyAddedSinceInteractionHandler : IGetAllRecentlyAddedSinceInteractionHandler
    {
        public GetAllRecentlyAddedSinceInteractionHandler(
            ICheckOffHoursNewConnectionsServicePOM service,
            ILogger<GetAllRecentlyAddedSinceInteractionHandler> logger)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetAllRecentlyAddedSinceInteractionHandler> _logger;
        private readonly ICheckOffHoursNewConnectionsServicePOM _service;
        private IList<Models.RecentlyAddedProspect> RecentlyAddedProspects { get; set; } = new List<Models.RecentlyAddedProspect>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetAllRecentlyAddedSinceInteraction getAllRecentlyInteraction = interaction as GetAllRecentlyAddedSinceInteraction;
            IList<Models.RecentlyAddedProspect> recentlyAdded = _service.GetAllRecentlyAddedSince(getAllRecentlyInteraction.WebDriver, getAllRecentlyInteraction.NumOfHoursAgo, getAllRecentlyInteraction.TimezoneId);
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
