using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class OrphanedCloudResource
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OrphanedCloudResourceId { get; set; }
                
        public string? Arn { get; set; }
        public string? FriendlyName { get; set; }
        public string? ResourceId { get; set; }
        public string? UserId { get; set; }
        public string? ResourceName { get; set; }
        public string? Reason { get; set; }

    }
}
