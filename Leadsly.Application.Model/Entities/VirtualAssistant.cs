using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class VirtualAssistant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? VirtualAssistantId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public string HalId { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? HalUnitId { get; set; }
        public SocialAccount? SocialAccount { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public EcsService? EcsService { get; set; }
        public EcsTaskDefinition? EcsTaskDefinition { get; set; }
        public CloudMapDiscoveryService? CloudMapDiscoveryService { get; set; }
        public HalUnit? HalUnit { get; set; }
    }
}
