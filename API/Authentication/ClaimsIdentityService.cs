using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace API.Authentication
{
    public class ClaimsIdentityService : IClaimsIdentityService
    {
        public ClaimsIdentityService(ILogger<IClaimsIdentityService> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<IClaimsIdentityService> _logger;

        public async Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string email = user.Email;
            _logger.LogDebug("Generating claims identity for {email}.", email);

            // Retrieve user claims
            IList<Claim> userClaims = await userManager.GetClaimsAsync(user);
            // Retrieve user roles
            IList<string> userRoles = await userManager.GetRolesAsync(user);

            foreach (string userRole in userRoles)
            {
                userClaims.Add(new Claim(ApiConstants.Jwt.ClaimIdentifiers.Role, userRole));
                IdentityRole role = await roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        userClaims.Add(roleClaim);
                    }
                }
            }

            return new ClaimsIdentity(userClaims, JwtBearerDefaults.AuthenticationScheme);
        }
    }
}