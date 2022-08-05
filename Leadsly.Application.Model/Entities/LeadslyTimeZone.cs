using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Leadsly.Application.Model.Entities
{
    public class LeadslyTimeZone
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string LeadslyTimeZoneId { get; set; }

        public string TimeZoneId { get; set; } = string.Empty;
    }
}
