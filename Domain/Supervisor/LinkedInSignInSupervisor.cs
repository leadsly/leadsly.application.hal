using Domain.Models.Requests;
using Domain.Models.Responses;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using OpenQA.Selenium;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        private const string SignInUrl = "https://www.LinkedIn.com";

        public SignInResultResponse SignUserIn(LinkedInSignInRequest request, StringValues attemptCountHeader)
        {
            SignInResultResponse resp = default;
            try
            {
                resp = InitializeSignInProcedure(request, attemptCountHeader);
            }
            finally
            {
                if (resp.InvalidEmail == false && resp.InvalidPassword == false && resp.TwoFactorAuthRequired == false)
                {
                    this._logger.LogInformation("Closing browser after auth");
                    _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Auth);
                }
            }
            return resp;
        }

        private SignInResultResponse InitializeSignInProcedure(LinkedInSignInRequest request, StringValues attemptCountHeader)
        {
            string header = attemptCountHeader.ToString();
            if (header == string.Empty)
            {
                // close any auth browsers running to start from scratch
                _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.Auth);
            }

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.Auth, string.Empty, out bool isNewWebDriver);
            if (webDriver == null)
            {
                return null;
            }

            if (isNewWebDriver)
            {
                _linkedInPageFacade.LinkedInPage.NavigateToPage(webDriver, SignInUrl);
            }

            return SignIn(request.Username, request.Password, webDriver);
        }

        private SignInResultResponse SignIn(string username, string password, IWebDriver webDriver)
        {
            // wait until we either have signin container or homepage news feed
            SignInOperationResult signInOperationResult = _linkedInPageFacade.LinkedInPage.DetermineSignInStatus(webDriver);

            if (signInOperationResult == SignInOperationResult.None)
            {
                return null;
            }

            if (signInOperationResult == SignInOperationResult.Unknown)
            {
                return null;
            }

            if (signInOperationResult == SignInOperationResult.HomePage)
            {
                return new()
                {
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    UnexpectedErrorOccured = false
                };
            }

            if (signInOperationResult == SignInOperationResult.InvalidEmail)
            {
                HalOperationResult<IOperationResponse> reenterEmailResult = _linkedInPageFacade.LinkedInLoginPage.ReEnterEmail<IOperationResponse>(webDriver, username);
                if (reenterEmailResult.Succeeded == false)
                {
                    return new()
                    {
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        UnexpectedErrorOccured = true
                    };
                }

                _humanBehaviorService.RandomWaitMilliSeconds(500, 1050);

                // ensure password is still entered, otherwise re-enter it too
                HalOperationResult<IOperationResponse> reenterPasswordResult = _linkedInPageFacade.LinkedInLoginPage.ReEnterPasswordIfEmpty<IOperationResponse>(webDriver, password);
                if (reenterPasswordResult.Succeeded == false)
                {
                    return new()
                    {
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        UnexpectedErrorOccured = true
                    };
                }

                _humanBehaviorService.RandomWaitMilliSeconds(300, 1350);
                _linkedInPageFacade.LinkedInLoginPage.ReSignIn<IOperationResponse>(webDriver);
            }

            if (signInOperationResult == SignInOperationResult.InvalidPassword)
            {
                HalOperationResult<IOperationResponse> reenterPasswordResult = _linkedInPageFacade.LinkedInLoginPage.ReEnterPassword<IOperationResponse>(webDriver, password);
                if (reenterPasswordResult.Succeeded == false)
                {
                    return new()
                    {
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        UnexpectedErrorOccured = true
                    };
                }

                _humanBehaviorService.RandomWaitMilliSeconds(500, 1100);
                _linkedInPageFacade.LinkedInLoginPage.ReSignIn<IOperationResponse>(webDriver);
            }

            if (signInOperationResult == SignInOperationResult.SignIn)
            {
                HalOperationResult<IOperationResponse> enterEmailResult = _linkedInPageFacade.LinkedInLoginPage.EnterEmail<IOperationResponse>(webDriver, username);
                if (enterEmailResult.Succeeded == false)
                {
                    return new()
                    {
                        UnexpectedErrorOccured = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None
                    };
                }

                _humanBehaviorService.RandomWaitMilliSeconds(500, 1200);
                HalOperationResult<IOperationResponse> enterPasswordResult = _linkedInPageFacade.LinkedInLoginPage.EnterPassword<IOperationResponse>(webDriver, password);
                if (enterPasswordResult.Succeeded == false)
                {
                    return new()
                    {
                        UnexpectedErrorOccured = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None
                    };
                }

                _humanBehaviorService.RandomWaitMilliSeconds(500, 1100);
                _linkedInPageFacade.LinkedInLoginPage.SignIn<IOperationResponse>(webDriver);
            }

            AfterSignInResult afterSigninResult = _linkedInPageFacade.LinkedInPage.DetermineAfterSigninStatus(webDriver);

            if (afterSigninResult == AfterSignInResult.HomePage)
            {
                return new()
                {
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
                    TwoFactorAuthRequired = true,
                    TwoFactorAuthType = twoFactorAuthType,
                    UnexpectedErrorOccured = false
                };
            }

            if (afterSigninResult == AfterSignInResult.InvalidEmail)
            {
                return new()
                {
                    InvalidEmail = true,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    UnexpectedErrorOccured = false
                };
            }

            if (afterSigninResult == AfterSignInResult.InvalidPassword)
            {
                return new()
                {
                    InvalidPassword = true,
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

            _humanBehaviorService.RandomWaitMilliSeconds(400, 950);
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

            TwoFactorAuthResult twoFactorAuthResult = _linkedInPageFacade.LinkedInLoginPage.DetermineTwoFactorAuthStatus(webDriver);

            if (twoFactorAuthResult == TwoFactorAuthResult.Unknown)
            {
                return null;
            }

            if (twoFactorAuthResult == TwoFactorAuthResult.None)
            {
                return null;
            }

            if (twoFactorAuthResult == TwoFactorAuthResult.InvalidOrExpiredCode)
            {
                return new()
                {
                    FailedToEnterCode = false,
                    InvalidOrExpiredCode = true,
                    UnexpectedErrorOccured = false
                };
            }

            if (twoFactorAuthResult == TwoFactorAuthResult.ToastErrorMessage)
            {
                return new()
                {
                    FailedToEnterCode = false,
                    InvalidOrExpiredCode = false,
                    UnexpectedErrorOccured = true
                };
            }

            if (twoFactorAuthResult == TwoFactorAuthResult.UnexpectedError)
            {
                return new()
                {
                    FailedToEnterCode = false,
                    InvalidOrExpiredCode = false,
                    UnexpectedErrorOccured = true
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
