using API.Auth.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Auth
{
    interface IAccessToken
    {
        Task<ApplicationAccessToken> GenerateJwt(string userId, ClaimsIdentity identity, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions);
    }
}
