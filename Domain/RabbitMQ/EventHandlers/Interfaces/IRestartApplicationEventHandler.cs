using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers.Interfaces
{
    public interface IRestartApplicationEventHandler
    {
        Task OnRestartApplicationEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
