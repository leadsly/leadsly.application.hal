using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Campaigns
{
    [DataContract]
    public class ProspectListViewModel
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "existing", IsRequired = true)]
        public bool Existing { get; set; }

        [DataMember(Name = "searchUrls", IsRequired = false)]
        public List<string> SearchUrls { get; set; }
    }
}
