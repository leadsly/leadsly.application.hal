using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Models.Responses
{
    public class ConnectedNetworkProspectsResponse
    {
        public int TotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
