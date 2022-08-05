using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class PrimaryProspect
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string PrimaryProspectId { get; set; }
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public long AddedTimestamp { get; set; }
        public string PrimaryProspectListId { get; set; }
        public string Area { get; set; }
        public string EmploymentInfo { get; set; }
        public string SearchResultAvatarUrl { get; set; }
        public PrimaryProspectList PrimaryProspectList { get; set; }
    }
}
