using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    public class HalsProspectListPhasesPayload
    {
        public Dictionary<string, List<ProspectListBody>> ProspectListPayload { get; set; } = new();
    }
}
