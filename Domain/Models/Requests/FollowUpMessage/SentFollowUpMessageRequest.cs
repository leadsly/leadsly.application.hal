using Domain.Models.FollowUpMessage;

namespace Domain.Models.Requests.FollowUpMessage
{
    public class SentFollowUpMessageRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public SentFollowUpMessage Item { get; set; }
    }
}
