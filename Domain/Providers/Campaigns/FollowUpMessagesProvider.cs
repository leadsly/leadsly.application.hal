using Domain.Providers.Campaigns.Interfaces;
using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class FollowUpMessagesProvider : IFollowUpMessagesProvider
    {
        public FollowUpMessagesProvider(IDeserializerProvider deserializerProvider)
        {
            _deserializerProvider = deserializerProvider;
        }

        private readonly IDeserializerProvider _deserializerProvider;
        public void ExecuteFollowUpMessagesPhase(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            FollowUpMessagesBody bodyContent = _deserializerProvider.DeserializeFollowUpMessagesBody(message);


        }
    }
}
