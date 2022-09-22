using Domain.Models.Responses;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class AllInOneVirtualAssistantMessageBody : PublishMessageBody
    {
        [IgnoreDataMember]
        public ConnectedNetworkProspectsResponse PreviousMonitoredResponse { get; set; }

        [IgnoreDataMember]
        public Queue<NetworkingMessageBody> NetworkingMessages { get; set; }

        [IgnoreDataMember]
        public Queue<FollowUpMessageBody> FollowUpMessages { get; set; }
        [DataMember(IsRequired = false)]
        public DeepScanProspectsForRepliesBody DeepScanProspectsForReplies { get; set; }

        [DataMember]
        public CheckOffHoursNewConnectionsBody CheckOffHoursNewConnections { get; set; }
    }
}
