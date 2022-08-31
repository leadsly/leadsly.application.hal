using System.Collections.Generic;

namespace Domain.Models.Responses
{
    public class ContactedProspectsResponse
    {
        public IList<NetworkProspectResponse> Prospects { get; set; }
    }
}
