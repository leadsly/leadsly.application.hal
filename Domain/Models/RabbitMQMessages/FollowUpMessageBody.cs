using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class FollowUpMessageBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string ProspectName { get; set; }

        [DataMember]
        public string ProspectProfileUrl { get; set; }

        [DataMember]
        public string CampaignProspectId { get; set; }

        [DataMember]
        public string FollowUpMessageId { get; set; }

        [DataMember]
        public int OrderNum { get; set; }

    }

}
