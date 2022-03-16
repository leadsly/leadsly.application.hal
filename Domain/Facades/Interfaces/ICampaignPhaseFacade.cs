using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades.Interfaces
{
    public interface ICampaignPhaseFacade
    {
        HalOperationResult<T> ExecuteFollowUpMessagesPhase<T>(FollowUpMessagesBody message)
            where T : IOperationResponse;

    }
}
