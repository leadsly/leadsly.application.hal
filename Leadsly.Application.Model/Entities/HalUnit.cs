using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class HalUnit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string HalUnitId { get; set; }
        public string HalId { get; set; } = string.Empty;

        /// <summary>
        /// For example 7:00 AM
        /// </summary>
        public string StartHour { get; set; } = "8:00 AM";

        /// <summary>
        /// For example 8:00 PM
        /// </summary>
        public string EndHour { get; set; } = "7:00 PM";

        public string TimeZoneId { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = new();

        // TO DO REMOVE BEFORE MERGE
        public SocialAccount? SocialAccount { get; set; }
    }
}
