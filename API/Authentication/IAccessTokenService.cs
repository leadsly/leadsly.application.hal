using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IAccessTokenService
    {
        Task<ApplicationAccessToken> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity);

        ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken);

        Task<RenewAccessTokenResult> TryRenewAccessToken(string expiredAccessToken);
    }
}
