using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{        
    public class RunEcsTaskRequest
    {        
        public string LaunchType { get; set; }
        public string TaskDefinition { get; set; }        
        public string ClusterArn { get; set; }        
        public int Count { get; set; }        
        public string AssignPublicIp { get; set; }                
        public List<string> Subnets { get; set; }
    }    
}
