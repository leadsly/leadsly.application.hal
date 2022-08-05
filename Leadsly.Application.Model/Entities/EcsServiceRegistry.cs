using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class EcsServiceRegistry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string EcsServiceRegistryId { get; set; }
        public string? EcsServiceId { get; set; }
        [Required]
        public string RegistryArn { get; set; }
        public EcsService EcsService { get; set; }
    }
}
