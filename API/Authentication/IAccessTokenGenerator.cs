using API.Authentication.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IAccessTokenGenerator
    {
        Task<ApplicationAccessToken> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions);
    }
}
