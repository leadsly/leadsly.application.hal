using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        void Authenticate_Bot(string email, string password);
    }
}
