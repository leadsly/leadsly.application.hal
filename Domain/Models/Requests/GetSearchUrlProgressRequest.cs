namespace Domain.Models.Requests
{
    public class GetSearchUrlProgressRequest
    {
        public string RequestUrl { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string HalId { get; set; }
    }
}
