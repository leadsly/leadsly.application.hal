using Leadsly.Application.Model.Campaigns.interfaces;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.ProspectList
{
    public class PrimaryProspectListPayload : IPrimaryProspectListPayload
    {
        public IList<PrimaryProspectRequest> Prospects { get; set; }
        public OperationInformation OperationInformation { get; set; }
    }
}
