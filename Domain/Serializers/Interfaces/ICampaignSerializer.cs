using Leadsly.Application.Model.Campaigns;

namespace Domain.Serializers.Interfaces
{
    public interface ICampaignSerializer
    {
        public GetSentConnectionsUrlStatusPayload DeserializeSentConnectionsUrlStatuses(string json);

        public NewAcceptedCampaignProspectsPayload DeserializeNewAcceptedCampaignProspects(string json);

        public SearchUrlProgressResponse DeserializeSearchUrlsProgress(string json);
    }
}
