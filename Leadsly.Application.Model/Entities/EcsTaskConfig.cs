using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class EcsTaskConfig
    {
        public string AssignPublicIp { get; set; }
        public string ClusterArn { get; set; }
        public int Count { get; set; }
        public string LaunchType { get; set; }
        public string TaskDefinition { get; set; }                
        public List<string> Subnets { get; set; }
    }
}
