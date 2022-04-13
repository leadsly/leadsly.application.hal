using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns.Interfaces
{
    public interface IFollowUpMessagesProvider
    {
        HalOperationResult<T> ExecutePhase<T>(FollowUpMessageBody message)
            where T : IOperationResponse;

    }
}
