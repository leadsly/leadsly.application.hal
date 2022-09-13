using Domain.Models.RabbitMQMessages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Domain.RabbitMQ.EventHandlers
{
    public abstract class RabbitMQEventHandlerBase
    {
        private readonly ILogger _logger;

        public RabbitMQEventHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual PublishMessageBody DeserializeMessage<T>(string rawMessage)
            where T : PublishMessageBody
        {
            _logger.LogInformation("Deserializing {0}", typeof(T).Name);
            T message = null;
            try
            {
                message = JsonConvert.DeserializeObject<T>(rawMessage);
                _logger.LogDebug("Successfully deserialized {0}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize {0}. Returning an explicit null", typeof(T).Name);
                return null;
            }

            return message;
        }
    }
}
