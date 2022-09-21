using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Models.Requests.MonitorForNewConnections
{
    public class UpdateConnectedNetworkProspectsRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public int TotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
