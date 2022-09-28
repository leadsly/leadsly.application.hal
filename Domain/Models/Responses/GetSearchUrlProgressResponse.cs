using Domain.Models.Networking;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.Models.Responses
{
    [DataContract]
    public class GetSearchUrlProgressResponse
    {
        [DataMember]
        public IList<SearchUrlProgressModel> Items { get; set; }
    }
}
