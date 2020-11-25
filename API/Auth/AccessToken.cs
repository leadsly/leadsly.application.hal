using API.Auth.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Auth
{
    public class AccessToken : IAccessToken
    {
        public async Task<ApplicationAccessToken> GenerateJwt(string userId, ClaimsIdentity identity, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions)
        {
            return new ApplicationAccessToken
            {
                access_token = await jwtFactory.GenerateEncodedToken(userId, identity),
                expires_in = (long)jwtOptions.ValidFor.TotalSeconds
            };
        }
    }
}
