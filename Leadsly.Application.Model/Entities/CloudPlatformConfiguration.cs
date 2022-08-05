using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class CloudPlatformConfiguration
    {
        public string Region { get; set; }
        public string ApiServiceDiscoveryName { get; set; }
        public EcsServiceConfig EcsServiceConfig { get; set; }
        public EcsTaskConfig EcsTaskConfig { get; set; }
        public EcsTaskDefinitionConfig EcsTaskDefinitionConfig { get; set; }
        public CloudMapServiceDiscoveryConfig ServiceDiscoveryConfig { get; set; }
    }
}
