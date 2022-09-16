using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Models.Responses
{
    public class PreviouslyConnectedNetworkProspectsResponse
    {
        public int PreviousTotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
