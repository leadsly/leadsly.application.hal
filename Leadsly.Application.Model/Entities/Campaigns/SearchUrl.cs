using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class SearchUrl
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SearchUrlId { get; set; }
        public string Url { get; set; }
        public string PrimaryProspectListId { get; set; }
        public PrimaryProspectList PrimaryProspectList { get; set; }
    }
}
