using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    public class UpdateSearchUrlProgressRequest : BaseHalRequest
    {   
        public string SearchUrlProgressId { get; set; }                
        public string SearchUrl { get; set; }        
        public string WindowHandleId { get; set; }        
        public long LastActivityTimestamp { get; set; }        
        public bool StartedCrawling { get; set; }        
        public bool Exhausted { get; set; }
        public int TotalSearchResults { get; set; }
        public int LastPage { get; set; }        
        public int LastProcessedProspect { get; set; }
    }
}
