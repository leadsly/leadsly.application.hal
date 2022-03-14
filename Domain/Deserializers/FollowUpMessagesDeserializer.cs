using Domain.Deserializers.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Deserializers
{
    public class FollowUpMessagesDeserializer : IFollowUpMessagesDeserializer
    {
        public FollowUpMessagesDeserializer()
        {

        }
        public FollowUpMessagesBody DeserializeBody(string body)
        {
            FollowUpMessagesBody followUpMessageBody = null;
            try
            {
                followUpMessageBody = JsonConvert.DeserializeObject<FollowUpMessagesBody>(body);
            }
            catch(Exception ex)
            {

            }

            return followUpMessageBody;
        }
    }
}
