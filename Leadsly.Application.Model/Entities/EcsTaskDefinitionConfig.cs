using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class EcsTaskDefinitionConfig
    {
        public List<string> RequiresCompatibilities { get; set; }
        public List<EcsContainerDefinitionConfig> ContainerDefinitions { get; set; }
        public string Cpu { get; set; }
        public string Memory { get; set; }
        public string ExecutionRoleArn { get; set; }
        public string TaskRoleArn { get; set; }
        public string NetworkMode { get; set; }
    }
}
