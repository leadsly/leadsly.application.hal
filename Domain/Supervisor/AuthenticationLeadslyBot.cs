using Domain.Models;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using PageObjects;
using PageObjects.Pages;
using System;
using TwoFactorAuthType = Domain.Models.TwoFactorAuthType;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public TwoFactorAuthenticationResult EnterTwoFactorAuth(TwoFactorAuthentication request)
        {
            TwoFactorAuthenticationResult result = new()
            {
                Succeeded = false
            };

            if (GetWebDriverFromCache(request.WebDriverId, out object driver).Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = (IWebDriver)driver;

            return EnterTwoFactorAuthentication(webDriver, request.Code);
        }
        private TwoFactorAuthenticationResult EnterTwoFactorAuthentication(IWebDriver driver, string code)
        {
            TwoFactorAuthenticationResult result = new();

            LinkedInLoginPage linkedInLoginPage = new LinkedInLoginPage(driver, _logger);
            // notify user we need their authenticator code
            _logger.LogInformation("Entering user's two factor authentication code");
            linkedInLoginPage.EnterTwoFactorAuthCode(code);
            _logger.LogInformation("Submitting user's two factor authentication code");
            linkedInLoginPage.SubmitTwoFactorAuthCode();

            // while verification keeps failing we must keep asking user for new code
            if (linkedInLoginPage.SMSVerificationCodeErrorDisplayed)
            {
                _logger.LogWarning("Verification code entered was invalid or expired");
                // notify user we need their code
                result.Succeeded = false;
                result.InvalidOrExpiredCode = true;
                result.Failures.Add(new()
                {
                    Detail = "Verification code entered was invalid or expired",
                    Reason = "Something went wrong entering in two factor authentication code"
                });
                return result;
            }

            if (linkedInLoginPage.CheckIfUnexpectedViewRendered)
            {
                _logger.LogWarning("Unexpected view rendered after attempting to submit two factor authentication code");
                result.Succeeded = false;
                result.InvalidOrExpiredCode = false;
                result.DidUnexpectedErrorOccur = true;
                result.Failures.Add(new()
                {
                    Reason = "An unexpected error rendered",
                    Detail = "An unexpected error rendered in the view while trying to submit two factor authentication code"
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }
        private ConnectAccountResult GetWebDriverFromCache(string webDriverId, out object driver)
        {
            ConnectAccountResult result = new()
            {
                Succeeded = false
            };

            if (!_memoryCache.TryGetValue(webDriverId, out driver))
            {
                result.Failures.Add(new()
                {
                    Reason = "Webdriver was not found in memory cache",
                    Detail = $"Memory cache does not contain the webdriver instance with id: {webDriverId}"
                });
                return result;
            }
            result.Succeeded = true;
            return result;
        }
        public ConnectAccountResult AuthenticateAccount(AuthenticateAccount request)
        {
            ConnectAccountResult result = new()
            {
                Succeeded = false
            };

            if (GetWebDriverFromCache(request.WebDriverId, out object driver).Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = (IWebDriver)driver;

            LinkedInPage linkedInPage = this._leadslyBot.GoToLinkedIn(webDriver);

            if (linkedInPage.IsAuthenticationRequired)
            {
                result = Authenticate(webDriver, request.Username, request.Password);
                result.WebDriverId = request.WebDriverId;
                return result;
            }

            result.TwoFactorAuthRequired = linkedInPage.IsAuthenticationRequired;
            result.Succeeded = true;
            return result;
        }        
        private ConnectAccountResult Authenticate(IWebDriver driver, string email, string password)
        {
            LinkedInLoginPage loginPage = this._leadslyBot.Authenticate(driver, email, password);

            ConnectAccountResult result = new()
            {
                Succeeded = false
            };
            if (loginPage.ConfirmAccountDisplayed)
            {
                loginPage.ConfirmAccountInfo();
            }

            if (loginPage.SomethingUnexpectedHappenedToastDisplayed)
            {
                result.UnexpectedErrorOccured = true;
                result.TwoFactorAuthRequired = false;
                string linkedInErrorMessage = loginPage.SomethingUnexpectedHappenedToast.GetAttribute("message");
                result.Failures.Add(new()
                {
                   Reason = "LinkedIn displayed error toast message", 
                   Detail = $"LinkedIn error: {linkedInErrorMessage}"
                });
                return result;
            }

            if (loginPage.CheckIfUnexpectedViewRendered)
            {
                result.TwoFactorAuthRequired = false;
                result.UnexpectedErrorOccured = true;
                result.Failures.Add(new()
                {
                    Reason = "Something unexpected rendered",
                    Detail = "LinkedIn did not render the new feed, two factor auth app view or two factor auth sms view"
                });
                return result;
            }

            if (loginPage.IsTwoFactorAuthRequired)
            {
                result = DetermineTwoFactorAuthenticationType(loginPage);

                if (result.Succeeded == false)
                {
                    result.UnexpectedErrorOccured = true;
                    result.Failures.Add(new()
                    {
                        Reason = "Failed to determine two factor auth type",
                        Detail = "Two factor auth type expected was sms or app, neither was found"
                    });
                    return result;
                }
                
                return result;
            }

            result.Succeeded = true;
            return result;
        }        
        private ConnectAccountResult DetermineTwoFactorAuthenticationType(LinkedInLoginPage loginPage)
        {
            ConnectAccountResult result = new()
            {
                TwoFactorAuthRequired = true,
                Succeeded = true
            };

            if (Enum.GetName(loginPage.TwoFactorAuthenticationType) == Enum.GetName(TwoFactorAuthType.AuthenticatorApp))
            {
                result.TwoFactorAuthType = TwoFactorAuthType.AuthenticatorApp;
            }
            else if (Enum.GetName(loginPage.TwoFactorAuthenticationType) == Enum.GetName(TwoFactorAuthType.SMS))
            {
                result.TwoFactorAuthType = TwoFactorAuthType.SMS;
            }
            else
            {
                // something went wrong
                result.TwoFactorAuthType = TwoFactorAuthType.None;
                result.Succeeded = false;
            }

            return result;
        }        
    }
}
