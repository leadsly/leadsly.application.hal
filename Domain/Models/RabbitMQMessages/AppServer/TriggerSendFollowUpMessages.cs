namespace Domain.Models.RabbitMQMessages.AppServer
{
    public class TriggerSendFollowUpMessages
    {
        public string UserId { get; set; }
        public string HalId { get; set; }
    }
}
