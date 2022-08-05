using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Campaigns
{
    [DataContract]
    public class DelayViewModel
    {
        [DataMember(Name = "value", IsRequired = true)]
        public int Value { get; set; }

        [DataMember(Name = "unit", IsRequired = true)]
        public string Unit { get; set; }
    }
}
