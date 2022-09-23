using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface IAllInOneVirtualAssistantEventHandler
    {
        Task OnAllInOneVirtualAssistantEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
