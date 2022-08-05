using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class ProspectListBody : PublishMessageBody
    {
        [DataMember]
        public List<string> SearchUrls { get; set; } = new();
        [DataMember]
        public string ProspectListPhaseId { get; set; } = string.Empty;
        [DataMember]
        public string PrimaryProspectListId { get; set; } = string.Empty;
        [DataMember]
        public string CampaignProspectListId { get; set; } = string.Empty;
        [DataMember]
        public string CampaignId { get; set; } = string.Empty;
        /// <summary>
        /// Represents the last page on which we've collected prospects from. This is necessary to avoid sending out connections
        /// to prospects who are not in our database. We are assuming the results surfaced by the search url for prospect list
        /// and for send connections are the same.
        /// </summary>
        [DataMember]
        public int LastProspectListPage { get; set; }

        [DataMember]
        public string SocialAccountId { get; set; } = string.Empty;
    }
}
