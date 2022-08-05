using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class StopEcsTaskRequest
    {
        public string Cluster { get; set; }
        public string Reason { get; set; }
        public string Task { get; set; }
    }
}
