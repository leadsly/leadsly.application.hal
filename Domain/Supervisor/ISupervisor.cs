using Domain.Models;
using Domain.Models.Requests;
using Domain.Models.Responses;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Primitives;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);

        SignInResultResponse SignUserIn(LinkedInSignInRequest request, StringValues attemptCount);

        HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccountRequest request)
            where T : IOperationResponse;

        HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthenticationRequest twoFactorAuth)
            where T : IOperationResponse;

        TwoFactorAuthResultResponse EnterTwoFactorAuth(TwoFactorAuthRequest request);

    }
}
