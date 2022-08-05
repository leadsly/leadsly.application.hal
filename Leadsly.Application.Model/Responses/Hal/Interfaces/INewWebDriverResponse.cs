using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface INewWebDriverResponse : IOperationResponse
    {
        public string WebDriverId { get; }
    }
}
