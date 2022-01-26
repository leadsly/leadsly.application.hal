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
        public ClaimsIdentityService(ILogger<IClaimsIdentityService> logger, BotLeadslyUserManager userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private readonly ILogger<IClaimsIdentityService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly BotLeadslyUserManager _userManager;

        public async Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user)
        {
            string email = user.Email;
            _logger.LogDebug("Generating claims identity for {email}.", email);

            // Retrieve user claims
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
            // Retrieve user roles
            IList<string> userRoles = await _userManager.GetRolesAsync(user);

            foreach (string userRole in userRoles)
            {
                userClaims.Add(new Claim(ApiConstants.Jwt.ClaimIdentifiers.Role, userRole));
                IdentityRole role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
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