using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        TwoFactorAuthenticationResult VerifyTwoFactorAuthentication(ConnectAccountTwoFactorAuth twoFactorAuth);

        WebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);

        ConnectAccountResult AuthenticateAccount(AuthenticateAccount request);
    }
}
