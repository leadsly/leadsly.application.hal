using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.MonitorForNewProspects
{
    public class NewProspectAcceptedPayload : INewProspectAcceptedPayload
    {
        public IList<NewProspectConnectionRequest> NewProspectConnections { get; set; }
        public OperationInformation OperationInformation { get; set; }
    }
}
