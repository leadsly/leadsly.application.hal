namespace Domain.Models.DeepScanProspectsForReplies
{
    public class ProspectRepliedModel
    {
        public string CampaignProspectId { get; set; }
        public string ResponseMessage { get; set; }
        public long ResponseMessageTimestamp { get; set; }
        public string Name { get; set; }
    }
}
