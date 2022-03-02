using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using TwoFactorAuthType = Domain.Models.TwoFactorAuthType;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthentication request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };
                        
            result = _halAuthProvider.EnterTwoFactorAuthenticationCode<T>(request.Code);

            if(result.Succeeded == false)
            {
                return result;
            }
            
            result = _halAuthProvider.SubmitTwoFactorAuthCode<T>();

            if(result.Succeeded == false)
            {
                return result;
            }

            IEnterTwoFactorAuthCodeResponse response = new EnterTwoFactorAuthCodeResponse();

            // while verification keeps failing we must keep asking user for new code
            if (_halAuthProvider.SMSVerificationCodeErrorDisplayed)
            {
                _logger.LogWarning("Verification code entered was invalid or expired");
                // notify user we need their code                
                response.InvalidOrExpiredCode = true;
                // because nothing technically is wrong, its just the incorrect two factor auth code
                result.Succeeded = true;
                result.Value = (T)response;
                result.Failures.Add(new()
                {
                    Detail = "Verification code entered was invalid or expired",
                    Reason = "Something went wrong entering in two factor authentication code"
                });
                return result;
            }

            if (_halAuthProvider.CheckIfUnexpectedViewRendered)
            {
                _logger.LogWarning("Unexpected view rendered after attempting to submit two factor authentication code");                
                response.InvalidOrExpiredCode = false;
                response.DidUnexpectedErrorOccur = true;
                result.Value = (T)response;
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
        public HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccount request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            result = this._halAuthProvider.GoToPage<T>(request.ConnectAuthUrl);

            if(result.Succeeded == false)
            {
                return result;
            }

            IConnectAccountResponse response = new ConnectAccountResponse
            {
                TwoFactorAuthRequired = this._halAuthProvider.IsAuthenticationRequired
            };
            result.Value = (T)response;

            if (this._halAuthProvider.IsAuthenticationRequired)
            {
                result = _halAuthProvider.Authenticate<T>(request.Username, request.Password);                
                return result;
            }

            result.Succeeded = true;
            return result;
        }        
    }
}
