namespace Domain.Models.Requests.Networking
{
    public class GetNetworkingMessagesRequest
    {
        public string RequestUrl { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
    }
}
