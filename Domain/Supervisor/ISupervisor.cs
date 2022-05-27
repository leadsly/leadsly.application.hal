using Domain.Models;
using Domain.Models.Requests;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);

        HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccountRequest request)
            where T : IOperationResponse;

        HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthenticationRequest twoFactorAuth)
            where T : IOperationResponse;

    }
}
