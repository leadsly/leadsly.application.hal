using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class ListEcsTasksRequest
    {
        public string Cluster { get; set; }
        public string ContainerInstance { get; set; }
        public string Family { get; set; }
        public string LaunchType { get; set; }
        public string ServiceName { get; set; }
        public string DesiredStatus { get; set; }
    }
}
