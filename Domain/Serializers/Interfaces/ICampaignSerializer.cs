using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;

namespace Domain.Serializers.Interfaces
{
    public interface ICampaignSerializer
    {
        public GetSentConnectionsUrlStatusPayload DeserializeSentConnectionsUrlStatuses(string json);

        public NewAcceptedCampaignProspectsPayload DeserializeNewAcceptedCampaignProspects(string json);

        public SearchUrlProgressPayload DeserializeSearchUrlsProgress(string json);
    }
}
