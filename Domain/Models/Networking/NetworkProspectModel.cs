namespace Domain.Models.Networking
{
    /// <summary>
    /// This is the prospect that has accepted our connection invite and is now in our network
    /// </summary>
    public class NetworkProspectModel
    {
        public string Name { get; set; }
        public string LastFollowUpMessageContent { get; set; }
        public string CampaignProspectId { get; set; }
        public string ProspectProfileUrl { get; set; }
    }
}
