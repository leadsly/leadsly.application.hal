using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string OrganizationId { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<ApplicationUser> OrganizationUsers { get; set; }

    }
}
