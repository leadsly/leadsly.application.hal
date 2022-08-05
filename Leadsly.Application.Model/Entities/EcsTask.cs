using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class EcsTask
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? EcsTaskId { get; set; }

        public string? ContainerName { get; set; }
        public string? EcsServiceId { get; set; }
        public EcsService? EcsService { get; set; }
    }
}
