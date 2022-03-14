using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Deserializers.Interfaces
{
    public interface IFollowUpMessagesDeserializer
    {
        FollowUpMessagesBody DeserializeBody(string body);
    }
}
