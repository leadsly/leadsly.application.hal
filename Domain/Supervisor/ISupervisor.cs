using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        ConnectAccountResult ConnectAccountWithLinkedIn(ConnectAccount connectAccount);

        TwoFactorAuthenticationResult VerifyTwoFactorAuthentication(ConnectAccountTwoFactorAuth twoFactorAuth);

        WebDriverInformation CreateWebDriver(CreateWebDriver newWebDriver);

        bool DestroyWebDriver(DestroyWebDriver destroyWebDriver);
    }
}
