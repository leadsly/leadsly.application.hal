using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class CheckOffHoursNewConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string TimezoneId { get; set; }

        [DataMember(IsRequired = false)]
        public int NumOfHoursAgo { get; set; }
    }
}
