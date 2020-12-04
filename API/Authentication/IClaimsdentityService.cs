using Domain.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
