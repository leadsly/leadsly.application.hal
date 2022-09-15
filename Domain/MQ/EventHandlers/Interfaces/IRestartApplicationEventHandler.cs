using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers.Interfaces
{
    public interface IRestartApplicationEventHandler
    {
        Task OnRestartApplicationEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
