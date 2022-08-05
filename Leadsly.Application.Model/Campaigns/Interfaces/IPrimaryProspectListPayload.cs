using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.interfaces
{
    public interface IPrimaryProspectListPayload : IOperationResponse
    {
        public IList<PrimaryProspectRequest> Prospects { get; set; }
    }
}
