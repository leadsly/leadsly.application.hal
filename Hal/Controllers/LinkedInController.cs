using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Supervisor;
using Hal.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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
        public IActionResult SignIn(LinkedInSignInRequest request)
        {
            _logger.LogInformation("SignIn action executing.");
            HttpContext.Request.Headers.TryGetValue("X-Auth-Attempt-Count", out StringValues attemptCount);
            SignInResultResponse response = _supervisor.SignUserIn(request, attemptCount);

            return response == null ? BadRequest(ProblemDetailsDescriptions.SignInError) : Ok(response);
        }

        [HttpPost("2fa")]
        [AuthAttemptCount]
        public IActionResult EnterTwoFactorAuth([FromBody] TwoFactorAuthRequest request)
        {
            _logger.LogInformation("EnterTwoFactorAuth action executing.");

            TwoFactorAuthResultResponse response = _supervisor.EnterTwoFactorAuth(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.TwoFactorAuth) : Ok(response);
        }

        [HttpPost("email-challenge-pin")]
        [AuthAttemptCount]
        public IActionResult EnterEmailChallengePin([FromBody] EmailChallengePinRequest request)
        {
            _logger.LogInformation("EnterEmailChallengePin action executing.");
            EmailChallengePinResultResponse response = _supervisor.EnterEmailChallengePin(request);

            return response == null ? BadRequest(ProblemDetailsDescriptions.TwoFactorAuth) : Ok(response);
        }
    }
}
