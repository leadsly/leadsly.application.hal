using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.MQ.Services.Interfaces
{
    public interface IGetMQMessagesService
    {
        Queue<T> GetAllMessages<T>(string queueNameIn, string halId)
            where T : PublishMessageBody;
    }
}
