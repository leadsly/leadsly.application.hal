using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public interface IDeserializerProvider
    {
        FollowUpMessagesBody DeserializeFollowUpMessagesBody(string body);
    }
}
