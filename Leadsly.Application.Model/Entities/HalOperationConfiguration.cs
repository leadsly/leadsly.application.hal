using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities
{
    public class HalOperationConfiguration
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string HalOperationConfigurationId { get; set; }

        public DateTimeOffset StartWorkDay { get; set; }

        public DateTimeOffset EndWorkDay { get; set; }
        public string TimeZoneId { get; set; }
    }
}
