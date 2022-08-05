using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class CampaignProspect
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignProspectId { get; set; }
        public string PrimaryProspectId { get; set; }
        public string CampaignProspectListId { get; set; }
        public string ProfileUrl { get; set; }
        public string Name { get; set; }
        public bool Accepted { get; set; }
        public long AcceptedTimestamp { get; set; }
        public string CampaignId { get; set; }
        public bool FollowUpComplete { get; set; }
        public string? ResponseMessage { get; set; }
        public long LastFollowUpMessageSentTimestamp { get; set; }
        public bool ConnectionSent { get; set; }
        public long ConnectionSentTimestamp { get; set; }
        public bool Replied { get; set; }
        public long RepliedTimestamp { get; set; }
        public int SentFollowUpMessageOrderNum { get; set; }
        public bool FollowUpMessageSent { get; set; }
        public CampaignProspectList CampaignProspectList { get; set; }
        public PrimaryProspect PrimaryProspect { get; set; }
        public Campaign Campaign { get; set; }
        public ICollection<CampaignProspectFollowUpMessage>? FollowUpMessages { get; set; }

    }
}
