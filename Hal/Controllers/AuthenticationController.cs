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
        public IActionResult Authenticate([FromBody] AuthenticateAccount authAccount)
        {
            HalOperationResult<IConnectAccountResponse> result = _supervisor.AuthenticateAccount<IConnectAccountResponse>(authAccount);

            if(result.Succeeded == false)
            {
                //return BadRequest_Test();
                result.Failures.Add(new()
                {
                    Detail = "not",
                    Reason = "Working",
                    Code = Codes.AWS_API_ERROR
                });
                return BadRequest_LeadslyAuthenticationError(result.Failures);
            }

            return Ok(result);
        }

        [HttpPost("2fa")]
        [AllowAnonymous]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthentication twoFactorAuth)
        {
            HalOperationResult<IEnterTwoFactorAuthCodeResponse> result = _supervisor.EnterTwoFactorAuth<IEnterTwoFactorAuthCodeResponse>(twoFactorAuth);

            return Ok(result);
        }
    }
}
