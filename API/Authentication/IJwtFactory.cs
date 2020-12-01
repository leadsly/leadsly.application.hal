using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication.Jwt
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userId, ClaimsIdentity identity);
    }
}
