using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication
{
    public interface IGetIdentity
    {
        Task<ClaimsIdentity> GetClaimsIdentity(ApplicationUser userToVerify, string password);

        Task<ClaimsIdentity> GenerateClaimsIdentity(ApplicationUser user);
    }
}
