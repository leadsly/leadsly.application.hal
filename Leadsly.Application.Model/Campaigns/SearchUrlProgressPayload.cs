using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    public class SearchUrlProgressPayload : ISearchUrlProgressPayload
    {
        public OperationInformation OperationInformation { get; set; }
        public IList<SearchUrlProgressRequest> SearchUrlsProgress { get; set; }
    }
}
