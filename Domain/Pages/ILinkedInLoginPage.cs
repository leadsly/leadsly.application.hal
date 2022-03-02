using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Pages
{
    public interface ILinkedInLoginPage
    {
        public bool ConfirmAccountDisplayed { get; }
        public bool SomethingUnexpectedHappenedToastDisplayed { get; }
        public bool CheckIfUnexpectedViewRendered { get; }
        public bool SMSVerificationCodeErrorDisplayed { get; }
        public bool IsTwoFactorAuthRequired { get; }
        public TwoFactorAuthType TwoFactorAuthenticationType { get; }
        public IWebElement SomethingUnexpectedHappenedToast { get; }

        HalOperationResult<T> EnterEmail<T>(string email)
            where T : IOperationResponse;
        HalOperationResult<T> EnterPassword<T>(string password)
            where T : IOperationResponse;        
        HalOperationResult<T> SignIn<T>()
            where T : IOperationResponse;        
        HalOperationResult<T> EnterTwoFactorAuthCode<T>(string code)
            where T : IOperationResponse;
        HalOperationResult<T> SubmitTwoFactorAuthCode<T>()
            where T : IOperationResponse;
        HalOperationResult<T> ConfirmAccountInfo<T>()
            where T : IOperationResponse;       

    }
}
