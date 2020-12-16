using API.Authentication.Jwt;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IAccessTokenService
    {
        Task<ApplicationAccessToken> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity);

        ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken);

        Task<RenewAccessTokenResult> TryRenewAccessToken(string expiredAccessToken, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager);

        long GetExpiryTimeStamp(string accessToken);
    }
}
