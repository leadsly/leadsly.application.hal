using Domain.Models.Networking;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.Models.Responses
{
    [DataContract]
    public class GetSearchUrlProgressResponse
    {
        [DataMember(Name = "SearchUrlsProgress")]
        public IList<SearchUrlProgressModel> SearchUrls { get; set; }
    }
}
