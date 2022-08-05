using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.Interfaces
{
    public interface INewAcceptedCampaignProspectsPayload : IOperationResponse
    {
        public IList<CampaignProspect> Prospects { get; set; }
    }
}
