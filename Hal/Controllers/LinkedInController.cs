using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LinkedInController : ApiControllerBase
    {
        public LinkedInController(ISupervisor supervisor, ILogger<LinkedInController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<LinkedInController> _logger;

        [HttpPost("signin")]
        [AllowAnonymous]
        public IActionResult SignIn(LinkedInSignInRequest request)
        {
            SignInResultResponse response = _supervisor.SignUserIn(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.SignInError) : Ok(response);
        }

        [HttpPost("2fa")]
        [AllowAnonymous]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthRequest request)
        {
            TwoFactorAuthResultResponse response = _supervisor.EnterTwoFactorAuth(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.TwoFactorAuth) : Ok(response);
        }
    }
}
