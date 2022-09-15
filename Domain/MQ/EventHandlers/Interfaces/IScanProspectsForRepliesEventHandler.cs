using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface IScanProspectsForRepliesEventHandler
    {
        Task OnScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
