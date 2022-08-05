using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    public class NewAcceptedCampaignProspectsPayload : INewAcceptedCampaignProspectsPayload
    {
        public IList<CampaignProspect> Prospects { get; set; }
        public OperationInformation OperationInformation { get; set; }
    }
}
