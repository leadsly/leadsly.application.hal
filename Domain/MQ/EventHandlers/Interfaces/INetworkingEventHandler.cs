using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface INetworkingEventHandler
    {
        Task OnNetworkingEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
