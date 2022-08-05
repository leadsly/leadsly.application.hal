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
    public class SearchUrlDetailsRequest : BaseHalRequest
    {
        [DataMember(Name = "SearchUrlDetailsId")]
        public string SearchUrlDetailsId { get; set; }

        [DataMember(Name = "OriginalUrl")]
        public string OriginalUrl { get; set; }
        [DataMember(Name = "CurrentUrl")]
        public string CurrentUrl { get; set; }

        [DataMember(Name = "WindowHandleId")]
        public string WindowHandleId { get; set; }

        [DataMember(Name = "LastActivityTimestamp")]
        public long LastActivityTimestamp { get; set; }
        [DataMember(Name = "StartedCrawling")]
        public bool StartedCrawling { get; set; }
        [DataMember(Name = "FinishedCrawling")]
        public bool FinishedCrawling { get; set; }
    }
}
