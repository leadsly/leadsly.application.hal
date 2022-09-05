using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface IScanProspectsForRepliesEventHandler
    {
        Task OnScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
