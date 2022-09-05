using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface IFollowUpMessageEventHandler
    {
        Task OnFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
