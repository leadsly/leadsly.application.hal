namespace Domain.Models.RabbitMQMessages.AppServer
{
    public class TriggerScanProspectsForRepliesMessage
    {
        public string UserId { get; set; }
        public string HalId { get; set; }
    }
}
