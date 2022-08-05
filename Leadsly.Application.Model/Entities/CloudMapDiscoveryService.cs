using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class CloudMapDiscoveryService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CloudMapDiscoveryServiceId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ServiceDiscoveryId { get; set; }
        public string Arn { get; set; }
        public string Name { get; set; }
        public string NamespaceId { get; set; }
        public DateTime? CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? EcsServiceId { get; set; }
        public EcsService EcsService { get; set; }
    }
}
