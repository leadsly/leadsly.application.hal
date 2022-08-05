using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class CloudMapServiceDiscoveryConfig
    {
        public string Name { get; set; }
        public string NamespaceId { get; set; }
        public int DnsRecordTTL { get; set; }
        public string DnsRecordType { get; set; }
    }
}
