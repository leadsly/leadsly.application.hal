using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class SocialAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SocialAccountId { get; set; }
        [Required]
        public string UserId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? VirtualAssistantId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? HalUnitId { get; set; }
        public SocialAccountType AccountType { get; set; }
        [Required]
        public string? Username { get; set; }
        public bool RunProspectListFirst { get; set; } = false;
        public bool MonthlySearchLimitReached { get; set; } = false;

        /// <summary>
        /// Whether this social account is linked with virtual assistant
        /// </summary>
        public bool Linked { get; set; } = false;
        public ApplicationUser User { get; set; }
        public VirtualAssistant? VirtualAssistant { get; set; }
        public HalUnit HalDetails { get; set; }
        public ScanProspectsForRepliesPhase ScanProspectsForRepliesPhase { get; set; }
        public ConnectionWithdrawPhase ConnectionWithdrawPhase { get; set; }
        public MonitorForNewConnectionsPhase MonitorForNewProspectsPhase { get; set; }

        // TODO DELETE THESE BEFORE MERGE TO MAIN
        [Required]
        public bool ConfiguredWithUsersLeadslyAccount { get; set; }
        public SocialAccountCloudResource SocialAccountCloudResource { get; set; }
    }
}
