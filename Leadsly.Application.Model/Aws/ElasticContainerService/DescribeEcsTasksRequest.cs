using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class DescribeEcsTasksRequest
    {
        public string Cluster { get; set; }
        public List<string> Tasks { get; set; }
    }
}
