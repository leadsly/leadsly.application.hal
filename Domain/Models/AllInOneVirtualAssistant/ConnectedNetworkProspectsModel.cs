using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Models.AllInOneVirtualAssistant
{
    public class ConnectedNetworkProspectsModel
    {
        public int TotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
