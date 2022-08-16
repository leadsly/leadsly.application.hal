using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Supervisor;
using Hal.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

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
        [AuthAttemptCount]
        public async Task<IActionResult> SignIn(LinkedInSignInRequest request)
        {
            HttpContext.Request.Headers.TryGetValue("X-Auth-Attempt-Count", out StringValues attemptCount);
            SignInResultResponse response = _supervisor.SignUserIn(request, attemptCount);

            return response == null ? BadRequest(ProblemDetailsDescriptions.SignInError) : Ok(response);
        }

        [HttpPost("2fa")]
        [AuthAttemptCount]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthRequest request)
        {
            TwoFactorAuthResultResponse response = _supervisor.EnterTwoFactorAuth(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.TwoFactorAuth) : Ok(response);
        }

        [HttpPost("email-challenge-pin")]
        [AuthAttemptCount]
        public IActionResult EnterEmailChallengePinAuth([FromBody] EmailChallengePinRequest request)
        {
            EmailChallengePinResultResponse response = _supervisor.EnterEmailChallengePin(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.TwoFactorAuth) : Ok(response);
        }
    }
}
