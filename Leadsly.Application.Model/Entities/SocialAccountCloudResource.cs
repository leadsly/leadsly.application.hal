using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class SocialAccountCloudResource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SocialAccountCloudResourceId { get; set; }
        public string? SocialAccountId { get; set; }
        public string? HalId { get; set; }
        public SocialAccount SocialAccount { get; set; }
        public EcsService EcsService { get; set; }
        public EcsTaskDefinition EcsTaskDefinition { get; set; }
        public CloudMapDiscoveryService CloudMapDiscoveryService { get; set; }
    }
}
