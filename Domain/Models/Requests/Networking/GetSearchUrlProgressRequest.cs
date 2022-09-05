namespace Domain.Models.Requests.Networking
{
    public class GetSearchUrlProgressRequest
    {
        public string RequestUrl { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
    }
}
