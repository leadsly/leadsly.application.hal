using Domain.Models.Requests;
using Domain.Models.Responses;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        private const string SignInUrl = "https://www.LinkedIn.com";

        public SignInResultResponse SignUserIn(LinkedInSignInRequest request)
        {
            SignInResultResponse resp = default;
            try
            {
                resp = SignIn(request);
            }
            finally
            {
                if (resp.InvalidCredentials == false && resp.TwoFactorAuthRequired == false)
                {
                    this._logger.LogInformation("Closing browser after auth");
                    _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Auth);
                }
            }
            return resp;
        }

        private SignInResultResponse SignIn(LinkedInSignInRequest request)
        {
            IWebDriver webDriver = _webDriverProvider.CreateWebDriver(BrowserPurpose.Auth, string.Empty);
            if (webDriver == null)
            {
                return null;
            }

            _linkedInPageFacade.LinkedInPage.NavigateToPage(webDriver, SignInUrl);

            // wait until we either have signin container or homepage news feed
            SignInOperationResult signInOperationResult = _linkedInPageFacade.LinkedInPage.DetermineSignInStatus(webDriver);

            if (signInOperationResult == SignInOperationResult.Unknown)
            {
                return null;
            }

            if (signInOperationResult == SignInOperationResult.HomePage)
            {
                return new()
                {
                    InvalidCredentials = false,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    UnexpectedErrorOccured = false
                };
            }

            HalOperationResult<IOperationResponse> enterEmailResult = _linkedInPageFacade.LinkedInLoginPage.EnterEmail<IOperationResponse>(webDriver, request.Username);
            if (enterEmailResult.Succeeded == false)
            {
                return new()
                {
                    InvalidCredentials = false,
                    UnexpectedErrorOccured = true,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None
                };
            }

            HalOperationResult<IOperationResponse> enterPasswordResult = _linkedInPageFacade.LinkedInLoginPage.EnterPassword<IOperationResponse>(webDriver, request.Password);
            if (enterPasswordResult.Succeeded == false)
            {
                return new()
                {
                    InvalidCredentials = false,
                    UnexpectedErrorOccured = true,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None
                };
            }

            _linkedInPageFacade.LinkedInLoginPage.SignIn<IOperationResponse>(webDriver);

            AfterSignInResult afterSigninResult = _linkedInPageFacade.LinkedInPage.DetermineAfterSigninStatus(webDriver);

            if (afterSigninResult == AfterSignInResult.HomePage)
            {
                return new()
                {
                    InvalidCredentials = false,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    UnexpectedErrorOccured = false
                };
            }

            if (afterSigninResult == AfterSignInResult.TwoFactorAuthRequired)
            {
                TwoFactorAuthType twoFactorAuthType = _linkedInPageFacade.LinkedInLoginPage.TwoFactorAuthenticationType(webDriver);
                return new()
                {
                    InvalidCredentials = false,
                    TwoFactorAuthRequired = true,
                    TwoFactorAuthType = twoFactorAuthType,
                    UnexpectedErrorOccured = false
                };
            }

            if (afterSigninResult == AfterSignInResult.InvalidCredentials)
            {
                return new()
                {
                    InvalidCredentials = true,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    UnexpectedErrorOccured = false
                };
            }

            return null;
        }

        public TwoFactorAuthResultResponse EnterTwoFactorAuth(TwoFactorAuthRequest request)
        {
            TwoFactorAuthResultResponse resp = default;
            try
            {
                resp = EnterTwoFactorAuthCode(request);
            }
            finally
            {
                if (resp.FailedToEnterCode == false && resp.InvalidOrExpiredCode == false && resp.UnexpectedErrorOccured == false)
                {
                    this._logger.LogInformation("Closing browser after 2FA");
                    _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Auth);
                }
            }

            return resp;
        }

        private TwoFactorAuthResultResponse EnterTwoFactorAuthCode(TwoFactorAuthRequest request)
        {
            IWebDriver webDriver = _webDriverProvider.GetWebDriver(BrowserPurpose.Auth);
            if (webDriver == null)
            {
                return null;
            }

            HalOperationResult<IOperationResponse> enterCodeResult = _linkedInPageFacade.LinkedInLoginPage.EnterTwoFactorAuthCode<IOperationResponse>(webDriver, request.Code);
            if (enterCodeResult.Succeeded == false)
            {
                return new()
                {
                    FailedToEnterCode = true,
                    InvalidOrExpiredCode = false,
                    UnexpectedErrorOccured = false
                };
            }

            HalOperationResult<IOperationResponse> submitCodeResult = _linkedInPageFacade.LinkedInLoginPage.SubmitTwoFactorAuthCode<IOperationResponse>(webDriver);
            if (submitCodeResult.Succeeded == false)
            {
                return new()
                {
                    FailedToEnterCode = false,
                    InvalidOrExpiredCode = false,
                    UnexpectedErrorOccured = true
                };
            }

            if (_linkedInPageFacade.LinkedInLoginPage.CheckIfUnexpectedViewRendered(webDriver))
            {
                return new()
                {
                    FailedToEnterCode = false,
                    UnexpectedErrorOccured = true,
                    InvalidOrExpiredCode = false
                };
            }

            if (_linkedInPageFacade.LinkedInLoginPage.SMSVerificationCodeErrorDisplayed(webDriver))
            {
                return new()
                {
                    UnexpectedErrorOccured = false,
                    FailedToEnterCode = false,
                    InvalidOrExpiredCode = true
                };
            }

            return new()
            {
                FailedToEnterCode = false,
                UnexpectedErrorOccured = false,
                InvalidOrExpiredCode = false
            };
        }
    }
}
