using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class EcsServiceConfig
    {
        public string AssignPublicIp { get; set; }
        public string ClusterArn { get; set; }
        public int DesiredCount { get; set; }
        public List<string> Subnets { get; set; }
        public List<string> SecurityGroups { get; set; }
        public string SchedulingStrategy { get; set; }
        public string LaunchType { get; set; }
        public string ServiceName { get; set; }
        public string TaskDefinition { get; set; }
    }
}
