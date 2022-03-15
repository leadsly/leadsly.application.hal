using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        
        public HalOperationResult<T> EnterTwoFactorAuth<T>(TwoFactorAuthenticationRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new()
            {
                Succeeded = false
            };

            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = request.BrowserPurpose,
                RequestedWindowHandleId = request.WindowHandleId
            };

            return _halAuthProvider.EnterTwoFactorAuthenticationCode<T>(operationData, request);
        }

        public HalOperationResult<T> AuthenticateAccount<T>(AuthenticateAccountRequest request)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = request.BrowserPurpose
            };

            result = _halAuthProvider.Authenticate<T>(operationData, request);

            //// if operation succeeded and the browser was closed
            //if (result.Succeeded == true && result.Value.OperationInformation.BrowserClosed == true)
            //{
            //    // copy over the authenticated chrome profile
                
            //}

            return result;
        }        
    }
}
