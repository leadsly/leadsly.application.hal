using System.Collections.Generic;

namespace Domain.Models.Requests
{
    public class ProspectsRepliedRequest
    {
        public string HalId { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public IList<ProspectReplied> Prospects { get; set; }
    }
}
