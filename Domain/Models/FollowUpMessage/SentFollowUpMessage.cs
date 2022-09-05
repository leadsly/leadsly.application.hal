namespace Domain.Models.FollowUpMessage
{
    public class SentFollowUpMessage
    {
        public string CampaignProspectId { get; set; }
        public int MessageOrderNum { get; set; }
        public long ActualDeliveryDateTimeStamp { get; set; }
    }
}
