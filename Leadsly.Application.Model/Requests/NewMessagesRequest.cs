using System.Collections.Generic;

namespace Leadsly.Application.Model.Requests
{
    public class NewMessagesRequest
    {
        public IList<NewMessageRequest> NewMessages { get; set; } = new List<NewMessageRequest>();
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
    }
}
