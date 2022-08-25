using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Executors
{
    public interface IMessageExecutorHandler<TMessage> where TMessage : PublishMessageBody
    {
        Task<bool> ExecuteMessageAsync(TMessage message);
    }
}
