using Domain.Models.Networking;
using System.Collections.Generic;

namespace Domain.Models.Responses
{
    public class NetworkProspectsResponse
    {
        public IList<NetworkProspectModel> Items { get; set; }
    }
}
