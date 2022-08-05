using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response.Hal
{
    public interface IResponseBaseViewModel
    {
        public bool WindowTabClosed { get; set; }
        public string WindowHandleId { get; set; }
        public bool WebDriverError { get; set; }
        public string HalId { get; set; }
    }
}
