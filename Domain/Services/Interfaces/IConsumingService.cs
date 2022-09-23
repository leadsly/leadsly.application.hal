using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IConsumingService
    {
        /// <summary>
        /// Start consuming (getting messages).
        /// </summary>
        Task StartConsumingAsync();
        Task StartConsumingAsync_AllInOneVirtualAssistant();
    }
}
