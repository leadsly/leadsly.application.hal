﻿using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public interface ISupervisor
    {
        IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);            

        ConnectAccountResult AuthenticateAccount(AuthenticateAccount request);

        TwoFactorAuthenticationResult EnterTwoFactorAuth(TwoFactorAuthentication twoFactorAuth);
    }
}
