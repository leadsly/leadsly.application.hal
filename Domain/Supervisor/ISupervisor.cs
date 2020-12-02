using Domain.ViewModels;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
         Task<string> CreateUserAsync(RegisterUserModel registeredModel);
    }
}
