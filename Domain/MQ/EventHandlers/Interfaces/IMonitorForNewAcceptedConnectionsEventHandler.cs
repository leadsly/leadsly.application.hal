using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface IMonitorForNewAcceptedConnectionsEventHandler
    {
        Task OnMonitorForNewAcceptedConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
