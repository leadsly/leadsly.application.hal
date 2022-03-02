using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
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

        public HalAuthProvider(ILogger<HalAuthProvider> logger, IWebDriver driver, ILinkedInLoginPage linkedInLoginPage, ILinkedInPage linkedInPage)
        {
            _logger = logger;
            _driver = driver;
            _linkedInLoginPage = linkedInLoginPage;
            _linkedInPage = linkedInPage;
        }

        private readonly ILinkedInLoginPage _linkedInLoginPage;
        private readonly ILinkedInPage _linkedInPage;
        private readonly IWebDriver _driver;
        private readonly ILogger<HalAuthProvider> _logger;

        public bool SMSVerificationCodeErrorDisplayed => _linkedInLoginPage.SMSVerificationCodeErrorDisplayed;
        public bool CheckIfUnexpectedViewRendered => _linkedInLoginPage.CheckIfUnexpectedViewRendered;
        public bool IsAuthenticationRequired => _linkedInPage.IsAuthenticationRequired;

        public HalOperationResult<T> Authenticate<T>(string email, string password)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            result = _linkedInLoginPage.EnterEmail<T>(email);
            if(result.Succeeded == false)
            {
                return result;
            }

            result = _linkedInLoginPage.EnterPassword<T>(password);
            if (result.Succeeded == false)
            {
                return result;
            }

            result = _linkedInLoginPage.SignIn<T>();
            if (result.Succeeded == false)
            {
                return result;
            }

            return AuthenticateAccount<T>(email, password);
        }

        private HalOperationResult<T> AuthenticateAccount<T>(string email, string password)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (_linkedInLoginPage.ConfirmAccountDisplayed)
            {
                result = _linkedInLoginPage.ConfirmAccountInfo<T>();
                if (result.Succeeded == false)
                {
                    // TODO add logging here and perhaps a message
                    return result;
                }
            }

            
            if (_linkedInLoginPage.SomethingUnexpectedHappenedToastDisplayed)
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    UnexpectedErrorOccured = true,
                    TwoFactorAuthRequired = false
                };                
                
                string linkedInErrorMessage = _linkedInLoginPage.SomethingUnexpectedHappenedToast.GetAttribute("message");
                result.Failures.Add(new()
                {
                    Reason = "LinkedIn displayed error toast message",
                    Detail = $"LinkedIn error: {linkedInErrorMessage}"
                });

                result.Value = (T)response;
                return result;
            }

            if (_linkedInLoginPage.CheckIfUnexpectedViewRendered)
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    UnexpectedErrorOccured = true,
                    TwoFactorAuthRequired = false
                };
                result.Failures.Add(new()
                {
                    Reason = "Something unexpected rendered",
                    Detail = "LinkedIn did not render the new feed, two factor auth app view or two factor auth sms view"
                });

                result.Value = (T)response;
                return result;
            }

            if (_linkedInLoginPage.IsTwoFactorAuthRequired)
            {
                result = DetermineTwoFactorAuthenticationType<T>();

                if (result.Succeeded == false)
                {
                    IConnectAccountResponse response = new ConnectAccountResponse()
                    {
                        UnexpectedErrorOccured = true
                    };

                    result.Failures.Add(new()
                    {
                        Reason = "Failed to determine two factor auth type",
                        Detail = "Two factor auth type expected was sms or app, neither was found"
                    });
                    result.Value = (T)response;
                    return result;
                }

                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> DetermineTwoFactorAuthenticationType<T>()
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (Enum.GetName(_linkedInLoginPage.TwoFactorAuthenticationType) == Enum.GetName(TwoFactorAuthType.AuthenticatorApp))
            {
                IConnectAccountResponse response = new ConnectAccountResponse()
                {
                    TwoFactorAuthType = TwoFactorAuthType.AuthenticatorApp
                };
                result.Value = (T)response;
            }
            else if (Enum.GetName(_linkedInLoginPage.TwoFactorAuthenticationType) == Enum.GetName(TwoFactorAuthType.SMS))
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

        public HalOperationResult<T> GoToPage<T>(string pageUrl)
            where T : IOperationResponse        
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            if (_driver.Url.Contains(pageUrl) == false)
            {
                result = _linkedInPage.GoToPage<T>(pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }

        public HalOperationResult<T> EnterTwoFactorAuthenticationCode<T>(string code)
            where T : IOperationResponse
        {
            _logger.LogInformation("Entering user's two factor authentication code");
            return _linkedInLoginPage.EnterTwoFactorAuthCode<T>(code);
        }

        public HalOperationResult<T> SubmitTwoFactorAuthCode<T>()
            where T : IOperationResponse
        {
            _logger.LogInformation("Submitting user's two factor authentication code");
            return _linkedInLoginPage.SubmitTwoFactorAuthCode<T>();
        }
    }
}
