using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Auth.Jwt
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(string userId, ClaimsIdentity identity);
    }
}
