using Leadsly.Application.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class SearchUrlProgressRequest : BaseHalRequest
    {
        [DataMember]
        public string SearchUrlProgressId { get; set; }
        [DataMember]
        public string WindowHandleId { get; set; }
        [DataMember]
        public int LastPage { get; set; }
        [DataMember]
        public int LastProcessedProspect { get; set; }
        [DataMember]
        public string SearchUrl { get; set; }
        [DataMember]
        public bool StartedCrawling { get; set; }
        [DataMember]
        public int TotalSearchResults { get; set; }
        [DataMember]
        public bool Exhausted { get; set; }
        [DataMember]
        public string CampaignId { get; set; }
        [DataMember]
        public long LastActivityTimestamp { get; set; }
    }
}
