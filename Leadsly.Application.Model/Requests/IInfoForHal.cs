using Leadsly.Application.Model.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests
{
    public interface IInfoForHal
    {
        public BrowserPurpose BrowserPurpose { get; set; }        
        public long AttemptNumber { get; set; }        
        public string WindowHandleId { get; set; }
    }
}
