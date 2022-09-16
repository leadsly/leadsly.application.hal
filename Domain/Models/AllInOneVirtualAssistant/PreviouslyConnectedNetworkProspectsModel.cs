using Domain.Models.MonitorForNewProspects;
using System.Collections.Generic;

namespace Domain.Models.AllInOneVirtualAssistant
{
    public class PreviouslyConnectedNetworkProspectsModel
    {
        public int PreviousTotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> Items { get; set; }
    }
}
