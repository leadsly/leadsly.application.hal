using Domain.Models.Responses;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class AllInOneVirtualAssistantMessageBody : PublishMessageBody
    {
        [IgnoreDataMember]
        public PreviouslyConnectedNetworkProspectsResponse PreviousMonitoredResponse { get; set; }

        [IgnoreDataMember]
        public Queue<NetworkingMessageBody> NetworkingMessages { get; set; }

        [IgnoreDataMember]
        public Queue<FollowUpMessageBody> FollowUpMessages { get; set; }
    }
}
