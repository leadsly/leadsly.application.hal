using Domain.Models.DeepScanProspectsForReplies;
using System.Collections.Generic;

namespace Domain.Models.Requests.DeepScanProspectsForReplies
{
    public class ProspectsRepliedRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public IList<ProspectRepliedModel> Items { get; set; }
    }
}
