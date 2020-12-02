using Domain.Models;
using Domain.Supervisor;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class AuthController : APIControllerBase
    {
        public AuthController(ISupervisor supervisor, ILogger<AuthController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<AuthController> _logger;

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] RegisterUserModel registerModel, CancellationToken ct = default)
        {
            _logger.LogDebug("Signup action executed.");

            ApplicationUser registeredUser = new ApplicationUser
            {
                Email = registerModel.Email              
            };

            string userId = await _supervisor.CreateUserAsync(registerModel);

            return null;
        }
    }
}
