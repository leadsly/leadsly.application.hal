using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.ViewModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class Campaign
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignId { get; set; }
        public string ApplicationUserId { get; set; }
        public string HalId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; } = true;
        public bool Expired { get; set; } = false;
        public long CreatedTimestamp { get; set; }
        public long StartTimestamp { get; set; }
        public long EndTimestamp { get; set; }
        public string? Notes { get; set; }
        public int DailyInvites { get; set; }
        public CampaignTypeEnum CampaignType { get; set; }
        public bool IsWarmUpEnabled { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public ProspectListPhase ProspectListPhase { get; set; }
        public FollowUpMessagePhase FollowUpMessagePhase { get; set; }
        public SendConnectionRequestPhase SendConnectionRequestPhase { get; set; }
        public SendEmailInvitePhase SendEmailInvitePhase { get; set; }
        public ICollection<FollowUpMessage> FollowUpMessages { get; set; }
        public ICollection<SendConnectionsStage> SendConnectionStages { get; set; }
        public CampaignProspectList CampaignProspectList { get; set; }
        public ICollection<SearchUrlDetails> SentConnectionsStatuses { get; set; }
        public ICollection<SearchUrlProgress> SearchUrlsProgress { get; set; }

    }
}
