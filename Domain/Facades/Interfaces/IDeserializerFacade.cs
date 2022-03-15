using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades.Interfaces
{
    public interface IDeserializerFacade
    {
        FollowUpMessagesBody DeserializeFollowUpMessagesBody(string body);
    }
}
