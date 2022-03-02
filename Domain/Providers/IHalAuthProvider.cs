using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public interface IHalAuthProvider
    {
        HalOperationResult<T> Authenticate<T>(string email, string password)
            where T : IOperationResponse;

        HalOperationResult<T> GoToPage<T>(string pageUrl)
            where T : IOperationResponse;

        HalOperationResult<T> EnterTwoFactorAuthenticationCode<T>(string code)
            where T : IOperationResponse;

        HalOperationResult<T> SubmitTwoFactorAuthCode<T>()
            where T : IOperationResponse;

        public bool SMSVerificationCodeErrorDisplayed { get; }
        public bool CheckIfUnexpectedViewRendered { get; }
        public bool IsAuthenticationRequired { get; }
        
    }
}
