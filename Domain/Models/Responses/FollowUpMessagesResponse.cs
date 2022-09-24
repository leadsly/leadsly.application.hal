using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.Models.Responses
{
    [DataContract]
    public class FollowUpMessagesResponse
    {
        [DataMember]
        public IList<FollowUpMessageBody> Items { get; set; }
    }
}
