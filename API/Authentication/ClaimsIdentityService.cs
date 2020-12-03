using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace API.Authentication
{
    public class ClaimsIdentityService : IClaimsIdentityService
    {
        public ClaimsIdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<IClaimsIdentityService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IClaimsIdentityService> _logger;

        public async Task<ClaimsIdentity> GetClaimsIdentityAsync(ApplicationUser userToVerify, string password)
        {
            if (userToVerify == null || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            if (userToVerify == null) return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await GenerateClaimsIdentityAsync(userToVerify);
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }

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
                userClaims.Add(new Claim(APIConstants.Jwt.ClaimIdentifiers.Role, userRole));
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