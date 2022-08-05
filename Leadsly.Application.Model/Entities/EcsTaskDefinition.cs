using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class EcsTaskDefinition
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsTaskDefinitionId { get; set; }
        public string Family { get; set; }
    }
}
