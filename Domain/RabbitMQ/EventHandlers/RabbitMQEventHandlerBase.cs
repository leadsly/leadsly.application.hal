using Leadsly.Application.Model.Campaigns;

namespace Domain.RabbitMQ.EventHandlers
{
    public abstract class RabbitMQEventHandlerBase
    {
        protected abstract PublishMessageBody DeserializeMessage(string rawMessage);
    }
}
