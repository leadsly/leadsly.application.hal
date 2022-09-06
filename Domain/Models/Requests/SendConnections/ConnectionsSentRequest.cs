using Domain.Models.SendConnections;
using System.Collections.Generic;

namespace Domain.Models.Requests.SendConnections
{
    public class ConnectionsSentRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public IList<ConnectionSentModel> Items { get; set; }
    }
}
