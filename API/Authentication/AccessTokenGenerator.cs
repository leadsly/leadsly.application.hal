using API.Authentication.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public class AccessTokenGenerator : IAccessTokenGenerator
    {
        public async Task<ApplicationAccessToken> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity, IJwtFactory jwtFactory, JwtIssuerOptions jwtOptions)
        {
            return new ApplicationAccessToken
            {
                access_token = await jwtFactory.GenerateEncodedJwtAsync(userId, identity),
                expires_in = (long)jwtOptions.ValidFor.TotalSeconds
            };
        }
    }
}
