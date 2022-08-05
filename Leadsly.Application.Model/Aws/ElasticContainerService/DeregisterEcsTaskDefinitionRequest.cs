using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class DeregisterEcsTaskDefinitionRequest
    {
        public string TaskDefinition { get; set; }
    }
}
