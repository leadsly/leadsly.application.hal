namespace Domain.Models.Responses
{
    /// <summary>
    /// This is the prospect that has accepted our connection invite and is now in our network
    /// </summary>
    public class NetworkProspectResponse
    {
        public string Name { get; set; }
        public string LastFollowUpMessageContent { get; set; }
        public string CampaignProspectId { get; set; }
        public string ProspectProfileUrl { get; set; }
    }
}
