using Domain.Models.Networking;

namespace Domain.Models.Requests.Networking
{
    public class UpdateSearchUrlProgressRequest
    {
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
        public string RequestUrl { get; set; }
        public SearchUrlProgressModel Item { get; set; }
    }
}
