using Leadsly.Application.Model.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class MonitorForNewAcceptedConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember(IsRequired = false)]
        public int NumOfHoursAgo { get; set; }
    }
}
