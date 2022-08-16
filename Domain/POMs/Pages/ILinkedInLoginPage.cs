using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;

namespace Domain.POMs.Pages
{
    public interface ILinkedInLoginPage
    {
        public bool ConfirmAccountDisplayed(IWebDriver webDriver);
        public bool SomethingUnexpectedHappenedToastDisplayed(IWebDriver webDriver);
        public bool CheckIfUnexpectedViewRendered(IWebDriver webDriver);
        public bool SMSVerificationCodeErrorDisplayed(IWebDriver webDriver);
        public bool IsTwoFactorAuthRequired(IWebDriver webDriver);
        public TwoFactorAuthType TwoFactorAuthenticationType(IWebDriver webDriver);
        public TwoFactorAuthResult DetermineTwoFactorAuthStatus(IWebDriver webDriver);
        public EmailChallengePinResult DetermineEmailChallengeStatus(IWebDriver webDriver);
        public IWebElement SomethingUnexpectedHappenedToast(IWebDriver webdriver);

        HalOperationResult<T> EnterEmail<T>(IWebDriver driver, string email)
            where T : IOperationResponse;
        HalOperationResult<T> ReEnterEmail<T>(IWebDriver driver, string email)
            where T : IOperationResponse;

        HalOperationResult<T> ReEnterPasswordIfEmpty<T>(IWebDriver driver, string password)
            where T : IOperationResponse;
        HalOperationResult<T> ReEnterPassword<T>(IWebDriver driver, string password)
            where T : IOperationResponse;
        HalOperationResult<T> EnterPassword<T>(IWebDriver driver, string password)
            where T : IOperationResponse;
        HalOperationResult<T> SignIn<T>(IWebDriver driver)
            where T : IOperationResponse;
        HalOperationResult<T> ReSignIn<T>(IWebDriver driver)
            where T : IOperationResponse;
        HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver driver, string code)
            where T : IOperationResponse;
        HalOperationResult<T> SubmitTwoFactorAuthCode<T>(IWebDriver driver)
            where T : IOperationResponse;
        HalOperationResult<T> EnterEmailChallengePin<T>(IWebDriver driver, string pin)
            where T : IOperationResponse;
        HalOperationResult<T> SubmitEmailChallengePin<T>(IWebDriver driver)
            where T : IOperationResponse;
        HalOperationResult<T> ConfirmAccountInfo<T>(IWebDriver driver)
            where T : IOperationResponse;

    }
}
