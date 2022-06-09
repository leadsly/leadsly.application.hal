using Domain.Facades.Interfaces;
using Domain.Models.Requests;
using Domain.POMs.Pages;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace Domain.Providers
{
    public class HalAuthProvider : IHalAuthProvider
    {

        public HalAuthProvider(ILogger<HalAuthProvider> logger, IWebDriverProvider webDriverProvider, ILinkedInPageFacade linkedInPageFacade)
        {
            _logger = logger;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
        }

        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;                      
        private readonly ILogger<HalAuthProvider> _logger;
        public const string DefaultUrl = "https://www.LinkedIn.com";

        public HalOperationResult<T> Authenticate<T>(WebDriverOperationData operationData, AuthenticateAccountRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<IGetOrCreateWebDriverOperation> driverResult = _webDriverProvider.CreateWebDriver<IGetOrCreateWebDriverOperation>(operationData);
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

            bool authRequired = _linkedInPageFacade.LinkedInPage.IsSignInContainerDisplayed(webDriver);
            if (authRequired == false)
            {
                // check if the news feed is displayed
                if(_linkedInPageFacade.LinkedInHomePage.IsNewsFeedDisplayed(webDriver) == false)
                {
                    webDriver.Dispose();

                    result.Succeeded = false;
                    return result;
                }

                result = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);

                IConnectAccountResponse response = new ConnectAccountResponse
                {
                    TwoFactorAuthRequired = authRequired,
                    OperationInformation = new OperationInformation
                    {
                        BrowserClosed = result.Succeeded,
                        TabClosed = result.Succeeded
                    }                    
                };                

                
                if (result.Succeeded == false)
                {
                    // close web driver if possible manually here
                    webDriver.Dispose();
                    response.OperationInformation.TabClosed = false;
                    response.OperationInformation.BrowserClosed = false;
                }
                else
                {
                    response.OperationInformation.TabClosed = true;
                    response.OperationInformation.BrowserClosed = true;
                }

                result.Value = (T)response;
                result.Succeeded = true;
                return result;
            }

            return EnterAuthenticationCredentials<T>(webDriver, request);
        }

        private HalOperationResult<T> EnterAuthenticationCredentials<T>(IWebDriver webDriver, AuthenticateAccountRequest request)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new();

            result = _linkedInPageFacade.LinkedInLoginPage.EnterEmail<T>(webDriver, request.Username);
            if(result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                                                      
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,                        
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,                        
                        OperationInformation = new OperationInformation
                        {
                            WebDriverError = true,
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true,
                        }
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)response;                    
                }                
                return result;
            }

            result = _linkedInPageFacade.LinkedInLoginPage.EnterPassword<T>(webDriver, request.Password);
            if (result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,                        
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,                        
                        OperationInformation = new OperationInformation
                        {
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true,
                            WebDriverError = true
                        }
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)response;
                }                
                return result;
            }

            result = _linkedInPageFacade.LinkedInLoginPage.SignIn<T>(webDriver);
            if (result.Succeeded == false)
            {
                if(result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse response = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,                        
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,                        
                        OperationInformation = new OperationInformation
                        {
                            WebDriverError = true,
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true,
                        }
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

            _linkedInPageFacade.LinkedInHomePage.WaitUntilNewsFeedIsDisplayed(webDriver);

            if (_linkedInPageFacade.LinkedInLoginPage.ConfirmAccountDisplayed(webDriver))
            {
                result = _linkedInPageFacade.LinkedInLoginPage.ConfirmAccountInfo<T>(webDriver);
                if (result.Succeeded == false)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                    
                    IConnectAccountResponse resp = new ConnectAccountResponse
                    {
                        UnexpectedErrorOccured = false,                        
                        TwoFactorAuthRequired = false,
                        TwoFactorAuthType = TwoFactorAuthType.None,                            
                        OperationInformation = new OperationInformation
                        {
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true,
                            WebDriverError = true,
                        }
                    };
                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                    return result;
                }
            }
            
            if (_linkedInPageFacade.LinkedInLoginPage.SomethingUnexpectedHappenedToastDisplayed(webDriver) == true)
            {
                string linkedInErrorMessage = _linkedInPageFacade.LinkedInLoginPage.SomethingUnexpectedHappenedToast(webDriver).GetAttribute("message");

                HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                
                IConnectAccountResponse resp = new ConnectAccountResponse
                {
                    UnexpectedErrorOccured = true,                    
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,                    
                    OperationInformation = new OperationInformation
                    {
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true,
                        WebDriverError = false
                    }
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

            if (_linkedInPageFacade.LinkedInLoginPage.CheckIfUnexpectedViewRendered(webDriver))
            {
                HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);                
                IConnectAccountResponse resp = new ConnectAccountResponse
                {
                    UnexpectedErrorOccured = true,                    
                    TwoFactorAuthRequired = false,
                    TwoFactorAuthType = TwoFactorAuthType.None,                    
                    OperationInformation = new OperationInformation
                    {
                        Failures = result.Failures,
                        BrowserClosed = closeBrowserResult.Succeeded,
                        TabClosed = closeBrowserResult.Succeeded,
                        ShouldOperationBeRetried = true,
                        WebDriverError = false
                    }
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

            if (_linkedInPageFacade.LinkedInLoginPage.IsTwoFactorAuthRequired(webDriver))
            {
                result = DetermineTwoFactorAuthenticationType<T>(webDriver);
                
                if (result.Succeeded == false)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);

                    IConnectAccountResponse failedToDetermineTwoFaResponse = new ConnectAccountResponse()
                    {
                        UnexpectedErrorOccured = !result.Succeeded,                        
                        TwoFactorAuthRequired = true,
                        TwoFactorAuthType = result.Succeeded ? ((IConnectAccountResponse)result.Value).TwoFactorAuthType : TwoFactorAuthType.NotDetermined,                        
                        OperationInformation = new OperationInformation
                        {
                            WindowHandleId = webDriver.CurrentWindowHandle,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true,
                            WebDriverError = false,
                            Failures = result.Failures
                        }
                    };
                    
                    result.ShouldOperationBeRetried = true;                    
                    result.Value = (T)failedToDetermineTwoFaResponse;
                    return result;
                }

                IConnectAccountResponse resp = new ConnectAccountResponse()
                {
                    UnexpectedErrorOccured = !result.Succeeded,                    
                    TwoFactorAuthRequired = true,                                 
                    TwoFactorAuthType = result.Succeeded ? ((IConnectAccountResponse)result.Value).TwoFactorAuthType : TwoFactorAuthType.NotDetermined,                    
                    OperationInformation = new OperationInformation
                    {
                        WindowHandleId = webDriver.CurrentWindowHandle,
                        ShouldOperationBeRetried = false,
                        Failures = result.Failures,
                        WebDriverError = false
                    }
                };
                result.Succeeded = true;
                result.Value = (T)resp;
                return result;
            }

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
                OperationInformation = new OperationInformation()
                {
                    BrowserClosed = browserClosed,
                    TabClosed = browserClosed,
                    ShouldOperationBeRetried = false,
                    WebDriverError = false                    
                }
                
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

            if (Enum.GetName(_linkedInPageFacade.LinkedInLoginPage.TwoFactorAuthenticationType(webDriver)) == Enum.GetName(TwoFactorAuthType.AuthenticatorApp))
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    TwoFactorAuthType = TwoFactorAuthType.AuthenticatorApp
                };
                result.Value = (T)response;
            }
            else if (Enum.GetName(_linkedInPageFacade.LinkedInLoginPage.TwoFactorAuthenticationType(webDriver)) == Enum.GetName(TwoFactorAuthType.SMS))
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

                result.Failures.Add(new()
                {
                    Reason = "Failed to determine two factor auth type",
                    Detail = "Two factor auth type expected was sms or app, neither was found"
                });
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
                result = _linkedInPageFacade.LinkedInPage.GoToPage<T>(webDriver, pageUrl);
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
            HalOperationResult<T> result = new();

            HalOperationResult<IGetOrCreateWebDriverOperation> driverResult = _webDriverProvider.GetWebDriver<IGetOrCreateWebDriverOperation>(operationData);
            if (driverResult.Succeeded == false)
            {
                result.Failures = driverResult.Failures;
                return result;
            }
            IWebDriver webDriver = driverResult.Value.WebDriver;

            // switch to the passed in window 
            result = _webDriverProvider.SwitchTo<T>(webDriver, request.WindowHandleId);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Consider closing browser window and re-trying. Perhaps commands are not successfully sent to the web driver");
                return result;
            }

            return EnterTwoFactorAuthCode<T>(webDriver, request);            
        }

        private HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver webDriver, TwoFactorAuthenticationRequest request) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            _logger.LogInformation("Entering user's two factor authentication code");
            result = _linkedInPageFacade.LinkedInLoginPage.EnterTwoFactorAuthCode<T>(webDriver, request.Code);
            if (result.Succeeded == false)
            {
                if (result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
                    IEnterTwoFactorAuthCodeResponse resp = new EnterTwoFactorAuthCodeResponse
                    {
                        UnexpectedErrorOccured = true,
                        InvalidOrExpiredCode = false,                        
                        OperationInformation = new OperationInformation
                        {
                            WebDriverError = false,
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true
                        }                        
                    };

                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                }
                return result;
            }

            result = _linkedInPageFacade.LinkedInLoginPage.SubmitTwoFactorAuthCode<T>(webDriver);
            if (result.Succeeded == false)
            {
                if (result.WebDriverError == true)
                {
                    HalOperationResult<T> closeBrowserResult = _webDriverProvider.CloseBrowser<T>(request.BrowserPurpose);
                    IEnterTwoFactorAuthCodeResponse resp = new EnterTwoFactorAuthCodeResponse
                    {
                        UnexpectedErrorOccured = true,
                        OperationInformation = new OperationInformation
                        {
                            WebDriverError = false,
                            Failures = result.Failures,
                            BrowserClosed = closeBrowserResult.Succeeded,
                            TabClosed = closeBrowserResult.Succeeded,
                            ShouldOperationBeRetried = true                            
                        }                        
                    };

                    result.ShouldOperationBeRetried = true;
                    result.Value = (T)resp;
                }
                return result;
            }

            if (_linkedInPageFacade.LinkedInLoginPage.SMSVerificationCodeErrorDisplayed(webDriver))
            {
                return HandleSMSVerificationCodeErrorDisplayed<T>(webDriver.CurrentWindowHandle);
            }

            if (_linkedInPageFacade.LinkedInLoginPage.CheckIfUnexpectedViewRendered(webDriver))
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

            IEnterTwoFactorAuthCodeResponse response = new EnterTwoFactorAuthCodeResponse
            {
                InvalidOrExpiredCode = false,
                UnexpectedErrorOccured = true
            };

            _logger.LogWarning("Unexpected view rendered after attempting to submit two factor authentication code");            
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
                       
            _logger.LogWarning("Verification code entered was invalid or expired");

            IEnterTwoFactorAuthCodeResponse response = new EnterTwoFactorAuthCodeResponse
            {
                InvalidOrExpiredCode = true,
                UnexpectedErrorOccured = false,
                OperationInformation = new OperationInformation
                {
                    WindowHandleId = currentWindowHandleId,
                    Failures = new()
                    {
                        new()
                        {
                            Code = Codes.WEBDRIVER_TWO_FA_CODE_ERROR,
                            Detail = "SMS verification code entered was invalid or expired",
                            Reason = "Something went wrong entering in two factor authentication code"
                        }                        
                    }
                    
                }
            };
            // notify user we need their code                
            result.Value = (T)response;            
            // because nothing technically is wrong, its just the incorrect two factor auth code
            result.Succeeded = true;   
            return result;
        }
    }
}
