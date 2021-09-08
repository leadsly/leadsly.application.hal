using Domain.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IAccessTokenService
    {
        Task<ApplicationAccessTokenViewModel> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity);

        ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken);

        Task<RenewAccessTokenResultViewModel> TryRenewAccessToken(string expiredAccessToken);
    }
}
