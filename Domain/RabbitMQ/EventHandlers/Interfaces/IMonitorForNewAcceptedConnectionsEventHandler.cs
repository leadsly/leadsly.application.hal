using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface IMonitorForNewAcceptedConnectionsEventHandler
    {
        Task OnMonitorForNewAcceptedConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
