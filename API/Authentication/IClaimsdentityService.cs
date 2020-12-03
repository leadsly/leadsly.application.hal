using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GetClaimsIdentityAsync(ApplicationUser user, string password);

        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
