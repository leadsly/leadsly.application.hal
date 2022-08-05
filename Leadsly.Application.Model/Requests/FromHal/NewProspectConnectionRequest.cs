using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class NewProspectConnectionRequest
    {
        [DataMember]
        public string ProspectName { get; set; }
        [DataMember]
        public string ProfileUrl { get; set; }
        [DataMember]
        public long AcceptedTimestamp { get; set; }
    }
}
