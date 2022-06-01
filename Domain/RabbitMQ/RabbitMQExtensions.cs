using Leadsly.Application.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace Domain.RabbitMQ
{
    public static class RabbitMQExtensions
    {
        public static void BasicNackRetry(this IModel channel, BasicDeliverEventArgs eventArgs)
        {
            var headers = eventArgs.BasicProperties.Headers;            
            if (headers != null)
            {
                headers.TryGetValue(RabbitMQConstants.DeliveryCount, out object count);

                if(count != null)
                {
                    int deliveryCount = Convert.ToInt32(count);
                    if (deliveryCount <= 3)
                    {
                        channel.BasicNack(eventArgs.DeliveryTag, false, true);
                        return;
                    }
                }
            }

            channel.BasicNack(eventArgs.DeliveryTag, false, false);
        }
    }
}
