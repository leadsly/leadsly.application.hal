using Leadsly.Application.Model;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [DataContract]
    public class WebDriverInformation : ResultBase, IWebDriverInformation
    {
        [DataMember]
        public string WebDriverId { get; set; }
    }
}
