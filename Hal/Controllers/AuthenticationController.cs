using Domain.Models;
using Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Hal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ApiControllerBase
    {
        public AuthenticationController(ISupervisor supervisor, ILogger<AuthenticationController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<AuthenticationController> _logger;

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Authenticate([FromBody] AuthenticateAccount authAccount)
        {
            ConnectAccountResult result = _supervisor.AuthenticateAccount(authAccount);                

            return Ok(result);
        }

        [HttpPost("2fa")]
        [AllowAnonymous]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthentication twoFactorAuth)
        {
            TwoFactorAuthenticationResult result = _supervisor.EnterTwoFactorAuth(twoFactorAuth);

            return Ok(result);
        }
    }
}
