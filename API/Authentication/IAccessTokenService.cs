using Domain.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IAccessTokenService
    {
        Task<ApplicationAccessTokenModel> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity);

        ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken);

        Task<RenewAccessTokenResultModel> TryRenewAccessToken(string expiredAccessToken);
    }
}
