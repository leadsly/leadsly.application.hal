namespace Domain.Models.Requests.DeepScanProspectsForReplies
{
    public class AllNetworkProspectsRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
    }
}
