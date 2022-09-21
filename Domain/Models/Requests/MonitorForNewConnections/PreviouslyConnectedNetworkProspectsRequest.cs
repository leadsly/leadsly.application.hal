namespace Domain.Models.Requests.MonitorForNewConnections
{
    public class PreviouslyConnectedNetworkProspectsRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
    }
}
