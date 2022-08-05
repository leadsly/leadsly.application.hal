using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Campaigns
{
    [DataContract]
    public class FollowUpMessageViewModel
    {
        [DataMember(Name = "content", IsRequired = true)]
        public string Content { get; set; }

        [DataMember(Name = "order", IsRequired = true)]
        public int Order { get; set; }

        [DataMember(Name = "delay", IsRequired = true)]
        public DelayViewModel Delay{ get; set; }

    }
}
