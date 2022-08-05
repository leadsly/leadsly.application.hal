using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class NewProspectsConnectionsAcceptedRequest : BaseHalRequest
    {
        [DataMember(Name = "NewAcceptedProspectsConnections")]
        public IList<NewProspectConnectionRequest> NewAcceptedProspectsConnections { get; set; }

        [DataMember(Name = "ApplicationUserId")]
        public string ApplicationUserId { get; set; }
    }
}
