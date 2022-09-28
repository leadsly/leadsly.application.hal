using Domain.MQ.Interfaces;
using Domain.MQ.Messages;
using Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Domain.MQ.Services
{
    public class GetMQMessagesService : IGetMQMessagesService
    {
        public GetMQMessagesService(
            ILogger<GetMQMessagesService> logger,
            IRabbitMQManager manager)
        {
            _logger = logger;
            _manager = manager;
        }

        private readonly ILogger<GetMQMessagesService> _logger;
        private readonly IRabbitMQManager _manager;

        public Queue<T> GetAllMessages<T>(string queueNameIn, string halId) where T : PublishMessageBody
        {
            Queue<T> mqMessages = new Queue<T>();
            BasicGetResult result = default;
            do
            {
                try
                {
                    result = _manager.GetMessage(queueNameIn, halId);
                    if (result != null)
                    {
                        T mqMessage = result.Body.Deserialize<T>();
                        if (mqMessage != null)
                        {
                            mqMessages.Enqueue(mqMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Perhaps there are no messages on the queue");
                }

            } while (result?.MessageCount > 0);

            return mqMessages;
        }
    }
}
