using Domain.Models;
using Domain.Supervisor;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses.Hal;
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
        public IActionResult Authenticate([FromBody] AuthenticateAccountRequest request)
        {
            HalOperationResult<IConnectAccountResponse> result = _supervisor.AuthenticateAccount<IConnectAccountResponse>(request);

            if(result.Succeeded == false)
            {
                return BadRequest_LeadslyAuthenticationError(result.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPost("2fa")]
        [AllowAnonymous]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthenticationRequest twoFactorAuth)
        {
            HalOperationResult<IEnterTwoFactorAuthCodeResponse> result = _supervisor.EnterTwoFactorAuth<IEnterTwoFactorAuthCodeResponse>(twoFactorAuth);

            if(result.Succeeded == false)
            {
                return BadRequest_LeadslyAuthenticationError(result.Failures);
            }

            return Ok(result.Value);
        }
    }
}
