using Domain.Models.ScanProspectsForReplies;
using System.Collections.Generic;

namespace Domain.Models.Requests.ScanProspectsForreplies
{
    public class NewMessagesRequest
    {
        public IList<NewMessageModel> Items { get; set; } = new List<NewMessageModel>();
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
    }
}
