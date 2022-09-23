using Domain.Models.Networking;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class DeepScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; } = string.Empty;

        [IgnoreDataMember]
        public IList<NetworkProspectModel> NetworkProspects { get; set; }
    }
}
