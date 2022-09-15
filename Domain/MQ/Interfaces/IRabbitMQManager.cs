using RabbitMQ.Client.Events;

namespace Domain.MQ.Interfaces
{
    public interface IRabbitMQManager
    {
        void StartConsuming(string queueNameIn, string routingKeyIn, string halId, AsyncEventHandler<BasicDeliverEventArgs> receivedHandlerAsync);

        void PublishMessage(byte[] body, string queueNameIn, string routingKeyIn);
    }
}
