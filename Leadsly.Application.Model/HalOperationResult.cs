using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public class HalOperationResult<T> : ResultBase
        where T : IOperationResponse
    {
        public T Value { get; set; }
        // these properties are for local operations, they do not get sent over.
        // Sometimes when an error occurs locally Value has not been set so this allows to tag errors and check them
        // without having to create Value object first
        public bool WebDriverError { get; set; } = false;        
        public bool ShouldOperationBeRetried { get; set; } = false;
    }
}
