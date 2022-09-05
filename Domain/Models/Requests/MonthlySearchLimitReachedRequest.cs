namespace Domain.Models.Requests
{
    public class MonthlySearchLimitReachedRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public bool MonthlySearchLimitReached { get; set; }
    }
}
