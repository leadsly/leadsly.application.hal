using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class ProspectsRepliedRequest : BaseHalRequest
    {
        [DataMember(Name = "ProspectsReplied", IsRequired = false)]
        public IList<ProspectRepliedRequest> ProspectsReplied { get; set; }
    }
}
