using Domain.Models.RabbitMQMessages;

namespace Domain.RabbitMQ.EventHandlers
{
    public abstract class RabbitMQEventHandlerBase
    {
        protected abstract PublishMessageBody DeserializeMessage(string rawMessage);
    }
}
