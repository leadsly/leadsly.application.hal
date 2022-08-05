using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class ProspectRepliedRequest
    {
        /// <summary>
        /// Used by DeepScanProspectsForRepliesPhase
        /// </summary>
        [DataMember(Name = "CampaignProspectId", IsRequired = false, EmitDefaultValue = true)]
        public string CampaignProspectId { get; set; }

        /// <summary>
        /// Used by DeepScanProspectsForRepliesPhase and ScanProspectsForRepliesPhase
        /// </summary>
        [DataMember]
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Used by DeepScanProspectsForRepliesPhase and ScanProspectsForRepliesPhase
        /// </summary>
        [DataMember]
        public long ResponseMessageTimestamp { get; set; }

        /// <summary>
        /// Used by ScanProspectsForRepliesPhase
        /// </summary>
        [DataMember]
        public string ProspectName { get; set; }

        /// <summary>
        /// Used by ScanProspectsForRepliesPhase
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string ProspectProfileUrl { get; set; }

    }
}
