using Domain.Models.Requests;
using Domain.Models.Responses;
using Microsoft.Extensions.Primitives;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        SignInResultResponse SignUserIn(LinkedInSignInRequest request, StringValues attemptCount);

        TwoFactorAuthResultResponse EnterTwoFactorAuth(TwoFactorAuthRequest request);

        EmailChallengePinResultResponse EnterEmailChallengePin(EmailChallengePinRequest request);

    }
}
