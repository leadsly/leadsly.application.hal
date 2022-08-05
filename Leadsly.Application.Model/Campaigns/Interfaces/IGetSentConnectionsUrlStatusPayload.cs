using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.Interfaces
{
    public interface IGetSentConnectionsUrlStatusPayload : IOperationResponse
    {
        public IList<SearchUrlDetailsRequest> SearchUrlDetailsRequests { get; set; }
    }
}
