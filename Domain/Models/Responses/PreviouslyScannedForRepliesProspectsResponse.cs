using Domain.Models.AllInOneVirtualAssistant;
using System.Collections.Generic;

namespace Domain.Models.Responses
{
    public class PreviouslyScannedForRepliesProspectsResponse
    {
        public IList<PreviouslyScannedForRepliesProspectModel> Items { get; set; }
    }
}
