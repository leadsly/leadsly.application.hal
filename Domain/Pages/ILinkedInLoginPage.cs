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
        public bool ConfirmAccountDisplayed(IWebDriver webDriver);
        public bool SomethingUnexpectedHappenedToastDisplayed(IWebDriver webDriver);
        public bool CheckIfUnexpectedViewRendered(IWebDriver webDriver);
        public bool SMSVerificationCodeErrorDisplayed(IWebDriver webDriver);
        public bool IsTwoFactorAuthRequired(IWebDriver webDriver);
        public TwoFactorAuthType TwoFactorAuthenticationType(IWebDriver webDriver);
        public IWebElement SomethingUnexpectedHappenedToast(IWebDriver webdriver);

        HalOperationResult<T> EnterEmail<T>(IWebDriver driver, string email)
            where T : IOperationResponse;
        HalOperationResult<T> EnterPassword<T>(IWebDriver driver, string password)
            where T : IOperationResponse;        
        HalOperationResult<T> SignIn<T>(IWebDriver driver)
            where T : IOperationResponse;        
        HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver driver, string code)
            where T : IOperationResponse;
        HalOperationResult<T> SubmitTwoFactorAuthCode<T>(IWebDriver driver)
            where T : IOperationResponse;
        HalOperationResult<T> ConfirmAccountInfo<T>(IWebDriver driver)
            where T : IOperationResponse;       

    }
}
