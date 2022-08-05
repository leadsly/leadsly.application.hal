using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.WebDriver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.WebDriver
{
    public class NewTabOperation : INewTabOperation
    {
        public string WindowHandleId { get; set; }
        public OperationInformation OperationInformation { get; set; }
    }
}
