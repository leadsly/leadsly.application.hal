namespace Domain.Models.Requests.FollowUpMessage
{
    public class GetFollowUpMessagesRequest
    {
        public string RequestUrl { get; set; }
        public string NamespaceName { get; set; }
        public string ServiceDiscoveryName { get; set; }
    }
}
