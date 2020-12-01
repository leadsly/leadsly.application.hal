using API.Authentication.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    interface IAccessToken
    {
        Task<ApplicationAccessToken> GenerateJwt(string userId, ClaimsIdentity identity, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions);
    }
}
