using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);

        HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccount request)
            where T : IOperationResponse;

        HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthentication twoFactorAuth)
            where T : IOperationResponse;
    }
}
