using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Aws.ElasticContainerService
{
    public class DeleteEcsServiceRequest
    {
        public string Cluster { get; set; }
        public bool Force { get; set; }
        public string Service { get; set; }
    }
}
