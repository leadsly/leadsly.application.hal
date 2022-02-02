using Leadsly.Models.Database;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain;

namespace API.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
