using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class UpdateEcsServiceRequest
    {
        public string ClusterArn { get; set; }
        public string ServiceName { get; set; }
        public int DesiredCount { get; set; }
    }
}
