using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface INetworkingEventHandler
    {
        Task OnNetworkingEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
