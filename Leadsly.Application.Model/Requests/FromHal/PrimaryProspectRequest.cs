using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class PrimaryProspectRequest
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ProfileUrl { get; set; }
        [DataMember]
        public long AddedTimestamp { get; set; }
        [DataMember]
        public string PrimaryProspectListId { get; set; }
        [DataMember]
        public string Area { get; set; }
        [DataMember]
        public string EmploymentInfo { get; set; }
        [DataMember]
        public string SearchResultAvatarUrl { get; set; }
    }
}
