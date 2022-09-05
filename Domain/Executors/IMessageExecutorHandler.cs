using Domain.Models.RabbitMQMessages;
using System.Threading.Tasks;

namespace Domain.Executors
{
    public interface IMessageExecutorHandler<TMessage> where TMessage : PublishMessageBody
    {
        Task<bool> ExecuteMessageAsync(TMessage message);
    }
}
