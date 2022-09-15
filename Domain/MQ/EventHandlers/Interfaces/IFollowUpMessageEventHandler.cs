using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface IFollowUpMessageEventHandler
    {
        Task OnFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
