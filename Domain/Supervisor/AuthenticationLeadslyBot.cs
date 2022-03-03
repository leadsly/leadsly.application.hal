using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public const string DefaultUrl = "https://www.LinkedIn.com";
        public HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthenticationRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            result = SwitchTo<T>(request.WindowHandleId, out string windowHandle);
            if(result.Succeeded == false)
            {
                return result;
            }
                        
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
            result.Value = (T)response;            

            // while verification keeps failing we must keep asking user for new code
            if (_halAuthProvider.SMSVerificationCodeErrorDisplayed)
            {
                _logger.LogWarning("Verification code entered was invalid or expired");
                // notify user we need their code                
                response.InvalidOrExpiredCode = true;
                // because nothing technically is wrong, its just the incorrect two factor auth code
                result.Succeeded = true;
                result.Value.WindowHandleId = windowHandle;
                result.Failures.Add(new()
                {
                    Detail = "SMS verification code entered was invalid or expired",
                    Reason = "Something went wrong entering in two factor authentication code"
                });
                return result;
            }

            if (_halAuthProvider.CheckIfUnexpectedViewRendered)
            {
                _logger.LogWarning("Unexpected view rendered after attempting to submit two factor authentication code");                
                response.InvalidOrExpiredCode = false;
                response.DidUnexpectedErrorOccur = true;

                _logger.LogDebug("Attempting to close the tab");
                CloseTab<T>(windowHandle);
                result.Value.WindowTabClosed = true;
                result.Failures.Add(new()
                {
                    Reason = "An unexpected error rendered",
                    Detail = "An unexpected error rendered in the view while trying to submit two factor authentication code"
                });                
                return result;
            }

            result = CloseTab<T>(result.Value.WindowHandleId);
            if(result.Succeeded == false)
            {
                result.Value.WebDriverError = true;
                return result;
            }

            result.Value.WindowHandleId = windowHandle;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccountRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            result = SwitchTo<T>(request.WindowHandleId, out string windowHandle);
            if(result.Succeeded == false)
            {
                return result;
            }

            result = this._halAuthProvider.GoToPage<T>(request.ConnectAuthUrl ?? DefaultUrl);

            if(result.Succeeded == false)
            {
                return result;
            }

            IConnectAccountResponse response = new ConnectAccountResponse
            {
                TwoFactorAuthRequired = this._halAuthProvider.IsAuthenticationRequired
            };
            result.Value = (T)response;    
            
            if (this._halAuthProvider.IsAuthenticationRequired == false)
            {
                result.Value.WindowHandleId = windowHandle;
                result = CloseTab<T>(result.Value.WindowHandleId);
                if (result.Succeeded == false)
                {
                    result.Value.WebDriverError = true;
                    return result;
                }

                result.Value.WindowTabClosed = true;
                result.Succeeded = true;
                return result;
            }

            result.Value.WindowHandleId = windowHandle;
            return _halAuthProvider.Authenticate<T>(request.Username, request.Password);                        
        }        
    }
}
