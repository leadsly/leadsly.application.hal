using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<string> CreateUserAsync(RegisterUserModel registerModel)
        {
            ApplicationUser newUser = new ApplicationUser
            {
                Email = registerModel.Email
            };

            IdentityResult result = null;//await _userManager.CreateAsync(newUser, registerModel.Password);

            return result.Succeeded ? newUser.Id : null;
        }
    }   
}
