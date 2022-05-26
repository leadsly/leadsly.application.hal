using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Serializers.Interfaces
{
    public interface ICampaignSerializer
    {
        public GetSentConnectionsUrlStatusPayload DeserializeSentConnectionsUrlStatuses(string json);

        public NewAcceptedCampaignProspectsPayload DeserializeNewAcceptedCampaignProspects(string json);

        public SearchUrlProgressResponse DeserializeSearchUrlsProgress(string json);
    }
}
