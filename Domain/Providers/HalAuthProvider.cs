using Domain.Models;
using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public class HalAuthProvider : IHalAuthProvider
    {

        public HalAuthProvider(ILogger<HalAuthProvider> logger, IWebDriverProvider webDriverProvider, ILinkedInLoginPage linkedInLoginPage, ILinkedInPage linkedInPage)
        {
            _logger = logger;
            _linkedInLoginPage = linkedInLoginPage;
            _linkedInPage = linkedInPage;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInLoginPage _linkedInLoginPage;
        private readonly IWebDriverManagerProvider _webDriverManagerProvider;
        private readonly ILinkedInPage _linkedInPage;
        private readonly ILogger<HalAuthProvider> _logger;
        public const string DefaultUrl = "https://www.LinkedIn.com";

        public HalOperationResult<T> Authenticate<T>(WebDriverOperationData operationData, AuthenticateAccountRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<ICreateWebDriverOperation> driverResult = _webDriverProvider.CreateWebDriver<ICreateWebDriverOperation>(operationData);
            if(driverResult.Succeeded == false)
            {
                HalOperationResult<T> result = new();
                result.Failures = driverResult.Failures;
                return result;
            }            
            IWebDriver webDriver = driverResult.Value.WebDriver;

            return AuthenticateUserSocialAccount<T>(webDriver, request);
        }

        private HalOperationResult<T> AuthenticateUserSocialAccount<T>(IWebDriver webDriver, AuthenticateAccountRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = GoToPage<T>(webDriver, request.ConnectAuthUrl ?? DefaultUrl);

            if(result.Succeeded == false)
            {
                return result;
            }

            bool authRequired = _linkedInPage.IsAuthenticationRequired(webDriver);

            if (authRequired == false)
            {
                result = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);

                IConnectAccountResponse response = new ConnectAccountResponse
                {
                    TwoFactorAuthRequired = authRequired,
                    BrowserClosed = result.Succeeded,
                    TabClosed = result.Succeeded
                };                

                result.Value = (T)response;
                if (result.Succeeded == false)
                {
                    // close web driver if possible manually here
                    webDriver.Dispose();
                    result.Value.TabClosed = false;
                    result.Value.BrowserClosed = false;
                }
                else
                {
                    result.Value.TabClosed = true;
                    result.Value.BrowserClosed = true;
                }                
                
                result.Succeeded = true;
                return result;
            }

            return EnterAuthenticationCredentials<T>(webDriver, request);
        }

        private HalOperationResult<T> EnterAuthenticationCredentials<T>(IWebDriver webDriver, AuthenticateAccountRequest request)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new();

            result = _linkedInLoginPage.EnterEmail<T>(webDriver, request.Username);
            if(result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                                                      
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,
                        WebDriverError = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)response;                    
                }                
                return result;
            }

            result = _linkedInLoginPage.EnterPassword<T>(webDriver, request.Password);
            if (result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,
                        WebDriverError = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)response;
                }                
                return result;
            }

            result = _linkedInLoginPage.SignIn<T>(webDriver);
            if (result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,
                        WebDriverError = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };

                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)response;
                }
                return result;
            }

            return AfterSignInClicked<T>(webDriver, request);
        }

        private HalOperationResult<T> AfterSignInClicked<T>(IWebDriver webDriver, AuthenticateAccountRequest request)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (_linkedInLoginPage.ConfirmAccountDisplayed(webDriver))
            {
                result = _linkedInLoginPage.ConfirmAccountInfo<T>(webDriver);
                if (result.Succeeded == false)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse resp = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,
                        WebDriverError = true,
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                    return result;
                }
            }
            
            if (_linkedInLoginPage.SomethingUnexpectedHappenedToastDisplayed(webDriver) == true)
            {
                string linkedInErrorMessage = _linkedInLoginPage.SomethingUnexpectedHappenedToast(webDriver).GetAttribute("message");

                HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                
                IConnectAccountResponse resp = new ConnectAccountResponse
                {
                    UnexpectedErrorOccured = true,
                    WebDriverError = false,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    Failures = result.Failures,
                    BrowserClosed = closeBrowserResult.Succeeded,
                    TabClosed = closeBrowserResult.Succeeded,
                    ShouldOperationBeRetried = true
                };
                
                result.Failures.Add(new()
                {
                    Reason = "LinkedIn displayed error toast message",
                    Detail = $"LinkedIn error: {linkedInErrorMessage}"
                });

                result.ShouldOperationBeRetried = true;
                result.Value = (T)resp;
                return result;
            }

            if (_linkedInLoginPage.CheckIfUnexpectedViewRendered(webDriver))
            {
                HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                
                IConnectAccountResponse resp = new ConnectAccountResponse
                {
                    UnexpectedErrorOccured = true,
                    WebDriverError = false,
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,
                    Failures = result.Failures,
                    BrowserClosed = closeBrowserResult.Succeeded,
                    TabClosed = closeBrowserResult.Succeeded,
                    ShouldOperationBeRetried = true
                };
                result.Failures.Add(new()
                {
                    Reason = "Something unexpected rendered",
                    Detail = "LinkedIn did not render the new feed, two factor auth app view or two factor auth sms view"
                });

                result.ShouldOperationBeRetried = true;
                result.Value = (T)resp;
                return result;                
            }

            if (_linkedInLoginPage.IsTwoFactorAuthRequired(webDriver))
            {
                result = DetermineTwoFactorAuthenticationType<T>(webDriver);

                IConnectAccountResponse resp = new ConnectAccountResponse()
                {
                    UnexpectedErrorOccured = !result.Succeeded,
                    WebDriverError = false,
                    TwoFactorAuthRequired = true,
                    Failures = result.Failures,
                    TwoFactorAuthType = result.Succeeded ? ((IConnectAccountResponse)result.Value).TwoFactorAuthType : TwoFactorAuthType.NotDetermined,
                    WindowHandleId = webDriver.CurrentWindowHandle,
                    ShouldOperationBeRetried = true
                };
                if (result.Succeeded == false)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);

                    resp = new ConnectAccountResponse()
                    {
                        UnexpectedErrorOccured = !result.Succeeded,
                        WebDriverError = false,
                        TwoFactorAuthRequired = true,
                        TwoFactorAuthType = result.Succeeded ? ((IConnectAccountResponse)result.Value).TwoFactorAuthType : TwoFactorAuthType.NotDetermined,
                        WindowHandleId = webDriver.CurrentWindowHandle,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };
                    
                    result.Failures.Add(new()
                    {
                        Reason = "Failed to determine two factor auth type",
                        Detail = "Two factor auth type expected was sms or app, neither was found"
                    });
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                    return result;
                }

                result.Succeeded = true;
                result.Value = (T)resp;
                return result;
            }

            // TODO verify user is authenticated
            result = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
            bool browserClosed = result.Succeeded;
            if(result.Succeeded == false)
            {
                try
                {
                    webDriver.Dispose();
                    browserClosed = true;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose of web driver manually");
                    browserClosed = false;
                }
            }

            IConnectAccountResponse response = new ConnectAccountResponse()
            {
                UnexpectedErrorOccured = false,
                TwoFactorAuthRequired = false,
                TwoFactorAuthType = TwoFactorAuthType.None,
                BrowserClosed = browserClosed,
                TabClosed = browserClosed
            };

            result.Value = (T)response;
            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> DetermineTwoFactorAuthenticationType<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (Enum.GetName(_linkedInLoginPage.TwoFactorAuthenticationType(webDriver)) == Enum.GetName(TwoFactorAuthType.AuthenticatorApp))
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    TwoFactorAuthType = TwoFactorAuthType.AuthenticatorApp
                };
                result.Value = (T)response;
            }
            else if (Enum.GetName(_linkedInLoginPage.TwoFactorAuthenticationType(webDriver)) == Enum.GetName(TwoFactorAuthType.SMS))
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    TwoFactorAuthType = TwoFactorAuthType.SMS
                };
                result.Value = (T)response;                
            }
            else
            {
                // something went wrong
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    TwoFactorAuthType = TwoFactorAuthType.None
                };
                result.Value = (T)response;
                result.Succeeded = false;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                result = _linkedInPage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }

        
        public HalOperationResult<T> EnterTwoFactorAuthenticationCode<T>(WebDriverOperationData operationData, TwoFactorAuthenticationRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<IGetWebDriverOperation> driverResult = _webDriverProvider.CreateWebDriver<IGetWebDriverOperation>(operationData);
            if (driverResult.Succeeded == false)
            {
                HalOperationResult<T> result = new();
                result.Failures = driverResult.Failures;
                return result;
            }
            IWebDriver webDriver = driverResult.Value.WebDriver;

            return EnterTwoFactorAuthCode<T>(webDriver, request);            
        }

        private HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver webDriver, TwoFactorAuthenticationRequest request) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            _logger.LogInformation("Entering user's two factor authentication code");
            result = _linkedInLoginPage.EnterTwoFactorAuthCode<T>(webDriver, request.Code);
            if (result.Succeeded == false)
            {
                if (result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
                    IEnterTwoFactorAuthCodeResponse resp = new EnterTwoFactorAuthCodeResponse
                    {
                        UnexpectedErrorOccured = true,
                        WebDriverError = false,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };

                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                }
                return result;
            }

            result = _linkedInLoginPage.SubmitTwoFactorAuthCode<T>(webDriver);
            if (result.Succeeded == false)
            {
                if (result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
                    IEnterTwoFactorAuthCodeResponse resp = new EnterTwoFactorAuthCodeResponse
                    {
                        UnexpectedErrorOccured = true,
                        WebDriverError = false,
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true
                    };

                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                }
                return result;
            }

            if (_linkedInLoginPage.SMSVerificationCodeErrorDisplayed(webDriver))
            {
                return HandleSMSVerificationCodeErrorDisplayed<T>(webDriver.CurrentWindowHandle);
            }

            if (_linkedInLoginPage.CheckIfUnexpectedViewRendered(webDriver))
            {
                result = HandleUnexpectedViewDisplayed<T>(request);
                if (result.Succeeded == false)
                {
                    try
                    {
                        webDriver.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to manually dispose of the web driver");
                    }
                }
                // there was an issue even if closing the web driver returned success
                result.Succeeded = false;
                return result;
            }

            result = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
            if (result.Succeeded == false)
            {
                try
                {
                    webDriver.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispose of web driver manually");
                }
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> HandleUnexpectedViewDisplayed<T>(TwoFactorAuthenticationRequest request) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IEnterTwoFactorAuthCodeResponse response = new EnterTwoFactorAuthCodeResponse();
            _logger.LogWarning("Unexpected view rendered after attempting to submit two factor authentication code");
            response.InvalidOrExpiredCode = false;
            response.UnexpectedErrorOccured = true;

            _logger.LogDebug("Attempting to close the tab");
            
            result = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
            result.Failures.Add(new()
            {
                Reason = "An unexpected error rendered",
                Detail = "An unexpected error rendered in the view while trying to submit two factor authentication code"
            });

            return result;
        }

        private HalOperationResult<T> HandleSMSVerificationCodeErrorDisplayed<T>(string currentWindowHandleId) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IEnterTwoFactorAuthCodeResponse response = new EnterTwoFactorAuthCodeResponse();
            response.InvalidOrExpiredCode = true;            

            _logger.LogWarning("Verification code entered was invalid or expired");
            // notify user we need their code                
            result.Value = (T)response;
            // because nothing technically is wrong, its just the incorrect two factor auth code
            result.Succeeded = true;
            result.Value.WindowHandleId = currentWindowHandleId;
            result.Failures.Add(new()
            {
                Detail = "SMS verification code entered was invalid or expired",
                Reason = "Something went wrong entering in two factor authentication code"
            });
            return result;
        }
    }
}
