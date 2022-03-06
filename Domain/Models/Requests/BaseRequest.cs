using Leadsly.Application.Model.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Requests
{
    [DataContract]
    public class BaseRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string WindowHandleId { get; set; }

        [DataMember(IsRequired = true)]
        public BrowserPurpose BrowserPurpose { get; set; }
    }
}
