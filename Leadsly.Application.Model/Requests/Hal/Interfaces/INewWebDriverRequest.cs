using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal.Interfaces
{
    public interface INewWebDriverRequest : IHalRequest
    {
        public int DefaultTimeoutInSeconds { get; }
    }
}
