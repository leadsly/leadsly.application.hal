﻿using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class CheckOffHoursNewConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }

        [DataMember(IsRequired = false)]
        public int NumOfHoursAgo { get; set; }
    }
}
