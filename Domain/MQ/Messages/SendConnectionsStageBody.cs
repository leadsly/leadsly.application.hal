using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class SendConnectionsStageBody
    {
        [DataMember(Name = "StartTime", IsRequired = true)]
        public string StartTime { get; set; }

        [DataMember(Name = "ConnectionsLimit", IsRequired = true)]
        public int ConnectionsLimit { get; set; }

        [DataMember(Name = "Order", IsRequired = true)]
        public int Order { get; set; }
    }
}
