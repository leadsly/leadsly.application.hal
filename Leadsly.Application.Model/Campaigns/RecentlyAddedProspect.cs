using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    public class RecentlyAddedProspect
    {
        public string Name { get; set; }
        public string ProfileUrl { get; set; }
        public int NumberOfHoursAgo { get; set; }
    }
}
