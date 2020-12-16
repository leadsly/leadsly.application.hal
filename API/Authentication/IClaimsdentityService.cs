using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager);
    }
}
