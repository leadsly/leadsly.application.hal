using Domain.Models;
using OpenQA.Selenium;
using PageObjects;
using PageObjects.Pages;
using TwoFactorAuthType = Domain.Models.TwoFactorAuthType;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public ConnectAccountResult ConnectAccountWithLinkedIn(ConnectAccount connectAccount)
        {
            WebDriverInformation driverInformation = _webDriverManager.Get(connectAccount.WebDriverId);
            IWebDriver driver = driverInformation.WebDriver;

            LinkedInPage linkedInPage = this._leadslyBot.GoToLinkedIn(driver);

            ConnectAccountResult result = new();
            if (linkedInPage.IsAuthenticationRequired)
            {
                result = Authenticate(driver, connectAccount.Email, connectAccount.Password);
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public TwoFactorAuthenticationResult VerifyTwoFactorAuthentication(ConnectAccountTwoFactorAuth twoFactorAuth)
        {
            WebDriverInformation driverInformation = _webDriverManager.Get(twoFactorAuth.WebDriverId);
            IWebDriver driver = driverInformation.WebDriver;

            TwoFactorAuthenticationResult result = EnterTwoFactorAuthentication(driver, twoFactorAuth.Code, twoFactorAuth.Email, twoFactorAuth.Password);
            
            return result;
        }

        private ConnectAccountResult Authenticate(IWebDriver driver, string email, string password)
        {
            LinkedInLoginPage loginPage = this._leadslyBot.Authenticate(driver, email, password);

            ConnectAccountResult result = new();
            if (loginPage.ConfirmAccountDisplayed)
            {
                loginPage.ConfirmAccountInfo();
            }

            if (loginPage.CheckIfUnexpectedViewRendered)
            {
                result.Succeeded = false;
                result.RequiresTwoFactorAuth = false;
                result.DidUnexpectedErrorOccur = true;
                return result;
            }

            if (loginPage.IsTwoFactorAuthRequired)
            {
                result = DetermineTwoFactorAuthenticationType(loginPage);

                if (result.Succeeded == false)
                {
                    result.DidUnexpectedErrorOccur = true;
                    return result;
                }
                
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private TwoFactorAuthenticationResult EnterTwoFactorAuthentication(IWebDriver driver, string code, string email, string password)
        {
            TwoFactorAuthenticationResult result = new();

            LinkedInLoginPage linkedInLoginPage = new LinkedInLoginPage(driver, _logger);
            // notify user we need their authenticator code
            linkedInLoginPage.EnterTwoFactorAuthCode(code);
            linkedInLoginPage.SubmitTwoFactorAuthCode();

            // while verification keeps failing we must keep asking user for new code
            if(linkedInLoginPage.DidVerificationCodeFailed)
            {
                // notify user we need their code
                result.Succeeded = false;
                result.InvalidOrExpiredCode = true;
                return result;
            }

            if (linkedInLoginPage.CheckIfUnexpectedViewRendered)
            {
                result.Succeeded = false;
                result.InvalidOrExpiredCode = false;
                result.DidUnexpectedErrorOccur = true;
                return result;
            }

            result.Succeeded = true;            
            return result;
        }

        private ConnectAccountResult DetermineTwoFactorAuthenticationType(LinkedInLoginPage loginPage)
        {
            ConnectAccountResult result = new()
            {
                RequiresTwoFactorAuth = true,
                Succeeded = true
            };

            if (loginPage.TwoFactorAuthenticationType.Equals(TwoFactorAuthType.AuthenticatorApp))
            {
                result.AuthType = TwoFactorAuthType.AuthenticatorApp;
            }
            else if (loginPage.TwoFactorAuthenticationType.Equals(TwoFactorAuthType.SMS))
            {
                result.AuthType = TwoFactorAuthType.SMS;
            }
            else
            {
                // something went wrong
                result.AuthType = TwoFactorAuthType.None;
                result.Succeeded = false;
            }

            return result;
        }
    }
}
