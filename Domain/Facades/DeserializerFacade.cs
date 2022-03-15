using Domain.Deserializers.Interfaces;
using Domain.Facades.Interfaces;
using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades
{
    public class DeserializerFacade : IDeserializerFacade
    {
        public DeserializerFacade(IFollowUpMessagesDeserializer followUpMessagesDeserializer)
        {
            _followUpMessagesDeserializer = followUpMessagesDeserializer;
        }

        private readonly IFollowUpMessagesDeserializer _followUpMessagesDeserializer;

        public FollowUpMessagesBody DeserializeFollowUpMessagesBody(string body)
        {
            return _followUpMessagesDeserializer.DeserializeBody(body);
        }
    }
}
