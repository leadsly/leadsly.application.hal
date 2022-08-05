using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class PrimaryProspectList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string PrimaryProspectListId { get; set; }
        public string Name { get; set; }
        public long CreatedTimestamp { get; set; }
        public string UserId { get; set; }
        public ICollection<SearchUrl> SearchUrls { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<PrimaryProspect> PrimaryProspects { get; set; }

    }
}
