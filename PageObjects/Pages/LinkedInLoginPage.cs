using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInLoginPage : ILinkedInLoginPage
    {
        private const string BaseUrl = "https://www.linkedin.com";

        public LinkedInLoginPage(ILogger<LinkedInLoginPage> logger)
        {
            this._logger = logger;
        }
        private readonly ILogger<LinkedInLoginPage> _logger;
        private TwoFactorAuthType? _authType = null;
        public TwoFactorAuthType TwoFactorAuthenticationType(IWebDriver webDriver)
        {
            return _authType ??= (TwoFactorAuthAppView(webDriver) != null ? TwoFactorAuthType.AuthenticatorApp : TwoFactorAuthType.SMS);
        }

        public bool SomethingUnexpectedHappenedToastDisplayed(IWebDriver webDriver)
        {
            return SomethingUnexpectedHappenedToast(webDriver) != null;
        }

        public IWebElement SomethingUnexpectedHappenedToast(IWebDriver webDriver)
        {

            IWebElement toast = null;
            try
            {
                toast = webDriver.FindElement(By.CssSelector("artdeco-toasts"));
                if (toast.GetAttribute("type") == "error")
                {
                    return toast;
                }
                return null;
            }
            catch (Exception ex)
            {

            }
            return toast;

        }

        public bool CheckIfUnexpectedViewRendered(IWebDriver webDriver)
        {
            if (HomePageView(webDriver) == null)
            {
                if (TwoFactorAuthAppView(webDriver) == null && TwoFactorAuthSMSView(webDriver) == null)
                {
                    return true;
                }
            }
            return false;

        }

        public IWebElement HomePageView(IWebDriver webDriver)
        {

            IWebElement homePageView = null;
            try
            {
                homePageView = webDriver.FindElement(By.Id("voyager-feed"));
            }
            catch (Exception ex)
            {

            }
            return homePageView;

        }

        public bool IsTwoFactorAuthRequired(IWebDriver webDriver)
        {
            if (TwoFactorAuthAppView(webDriver) != null)
            {
                _authType = TwoFactorAuthType.AuthenticatorApp;
                return true;
            }
            else if (TwoFactorAuthSMSView(webDriver) != null)
            {
                _authType = TwoFactorAuthType.SMS;
                return true;
            }
            _authType = TwoFactorAuthType.None;
            return false;

        }

        private IWebElement TwoFactorAuthenticationCodeInput(IWebDriver webDriver)
        {
            IWebElement twoFactorAuthenticationCodeInput = null;
            try
            {
                if (TwoFactorAuthenticationType(webDriver) == TwoFactorAuthType.AuthenticatorApp)
                {
                    twoFactorAuthenticationCodeInput = TwoFactorAuthAppView(webDriver).FindElement(By.Id("input__phone_verification_pin"));
                }
                else
                {
                    twoFactorAuthenticationCodeInput = TwoFactorAuthSMSView(webDriver).FindElement(By.Id("input__phone_verification_pin"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor authentication input control.");
            }
            return twoFactorAuthenticationCodeInput;

        }

        private IWebElement TwoStepAuthenticationSubmitButton(IWebDriver webDriver)
        {

            IWebElement twoFactorSubmitButton = null;
            try
            {
                if (TwoFactorAuthenticationType(webDriver) == TwoFactorAuthType.AuthenticatorApp)
                {
                    twoFactorSubmitButton = TwoFactorAuthAppView(webDriver).FindElement(By.CssSelector("#auth-app-div button#two-step-submit-button"));
                }
                else
                {
                    twoFactorSubmitButton = TwoFactorAuthSMSView(webDriver).FindElement(By.CssSelector("#app__container button#two-step-submit-button"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate two step authentication submit button.");
            }
            return twoFactorSubmitButton;

        }

        private IWebElement TwoFactorAuthAppView(IWebDriver webDriver)
        {
            IWebElement twoFactorAuthAppView = default;
            try
            {
                twoFactorAuthAppView = webDriver.FindElement(By.Id("auth-app-div"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor auth app view.");
            }

            return twoFactorAuthAppView;
        }

        private IWebElement TwoFactorAuthSMSView(IWebDriver webDriver)
        {

            IWebElement twoFactorAuthSMSView = default;
            try
            {
                twoFactorAuthSMSView = webDriver.FindElement(By.Id("app__container"));
                try
                {
                    // verify this is two factor auth id=input__phone_verification_pin 
                    IWebElement sms2faInput = twoFactorAuthSMSView.FindElement(By.Id("input__phone_verification_pin"));
                    if (sms2faInput == null)
                    {
                        twoFactorAuthSMSView = null;
                    }
                }
                catch (Exception ex)
                {
                    twoFactorAuthSMSView = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor auth sms view.");
            }

            return twoFactorAuthSMSView;

        }

        public bool ConfirmAccountDisplayed(IWebDriver webDriver)
        {
            string currentUrl = webDriver.Url;
            if (currentUrl.Contains("check/manage-account"))
            {
                // most likely we need to confirm our account before proceeding
                if (ConfirmButton(webDriver).Displayed)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DidVerificationCodeFailed(IWebDriver webDriver)
        {
            return VerificationCodeFailureBanner(webDriver) != null;

        }

        private IWebElement VerificationCodeFailureBanner(IWebDriver webDriver)
        {
            IWebElement verificationCodeFailureBanner = null;
            try
            {
                verificationCodeFailureBanner = webDriver.FindElement(By.CssSelector("span[role='alert']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[LinkedInLoginPage]: Failed to locate two factor authentication error banner. This is most likely desired behavior.");
            }
            return verificationCodeFailureBanner;
        }

        public bool SMSVerificationCodeErrorDisplayed(IWebDriver webDriver)
        {
            return SMSVerificationCodeError(webDriver) != null;            
        }

        private IWebElement SMSVerificationCodeError(IWebDriver webDriver)
        {

            IWebElement smsVerificationCodeError = default;
            try
            {
                smsVerificationCodeError = webDriver.FindElement(By.CssSelector(".app__content .body__banner-wrapper .body__banner--error span"));
                if (smsVerificationCodeError.GetAttribute("role") == "alert")
                {
                    if (smsVerificationCodeError.Displayed == false)
                    {
                        smsVerificationCodeError = null;
                    }
                    else
                    {
                        if (smsVerificationCodeError.Text.Contains("verification code") == false)
                        {
                            smsVerificationCodeError = null;
                        }
                    }

                }
            }
            catch (Exception ex)
            {

            }
            return smsVerificationCodeError;

        }

        private IWebElement SkipButton(IWebDriver webDriver)
        {

            IWebElement skipButton = null;
            IEnumerable<IWebElement> skipButtons = webDriver.FindElements(By.CssSelector("button[type='button']"));
            foreach (IWebElement skipBttn in skipButtons)
            {
                if (skipBttn.Text == "Skip")
                {
                    skipButton = skipBttn;
                    break;
                }
            }
            return skipButton;

        }

        private IWebElement ConfirmButton(IWebDriver webDriver)
        {

            IWebElement submitButton = null;
            IEnumerable<IWebElement> submitButtons = webDriver.FindElements(By.CssSelector("button[type='submit']"));
            foreach (IWebElement submitBttn in submitButtons)
            {
                if (submitBttn.Text == "Confirm")
                {
                    submitButton = submitBttn;
                    break;
                }
            }
            return submitButton;

        }

        private IWebElement LoginContainer(IWebDriver webDriver)
        {

            IWebElement loginContainer = default;
            try
            {
                loginContainer = webDriver.FindElement(By.CssSelector(".sign-in-form-container"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate loginContainer.");
            }

            return loginContainer;

        }

        private IEnumerable<IWebElement> CredentialInputs(IWebDriver webDriver)
        {

            IEnumerable<IWebElement> inputs = default;
            try
            {
                inputs = LoginContainer(webDriver)?.FindElements(By.CssSelector(".sign-in-form__form-input-container .input__input")) ?? throw new Exception();
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate credential inputs.");
            }

            return inputs;

        }

        private IWebElement EmailInput(IWebDriver webDriver)
        {

            IWebElement emailInput = null;
            try
            {
                foreach (IWebElement input in CredentialInputs(webDriver))
                {
                    if (input.GetAttribute("autocomplete") == "username")
                    {
                        _logger.LogDebug("[LinkedInLoginPage]: Username input field found!.");
                        emailInput = input;
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate username input field.");
            }

            return emailInput;

        }

        private IWebElement PasswordInput(IWebDriver webDriver)
        {

            IWebElement passwordInput = null;
            try
            {
                foreach (IWebElement input in CredentialInputs(webDriver))
                {
                    if (input.GetAttribute("autocomplete") == "current-password")
                    {
                        _logger.LogDebug("[LinkedInLoginPage]: Password input field found!.");
                        passwordInput = input;
                        break;
                    }
                }

                _ = passwordInput ?? throw new Exception();

            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate password input field.");
            }

            return passwordInput;

        }

        private IWebElement SignInButton(IWebDriver webDriver)
        {

            IWebElement signInButton = null;
            try
            {
                signInButton = webDriver.FindElement(By.CssSelector(".sign-in-form__submit-button"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate sign in button.");
            }

            return signInButton;

        }

        public HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver webDriver, string twoFactorAuthCode)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            Random rnd = new Random(300);
            foreach (char character in twoFactorAuthCode)
            {
                TwoFactorAuthenticationCodeInput(webDriver).SendKeys(character.ToString());
                Thread.Sleep(rnd.Next(500));
            }
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SubmitTwoFactorAuthCode<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            Random rnd = new Random(300);
            TwoStepAuthenticationSubmitButton(webDriver).Click();
            Thread.Sleep(rnd.Next(1000));

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> ConfirmAccountInfo<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            Random rnd = new Random(300);
            Thread.Sleep(rnd.Next(500));
            this.ConfirmButton(webDriver).Click();
            Thread.Sleep(rnd.Next(500));

            if (this.ConfirmAccountDisplayed(webDriver))
            {
                result = ConfirmAccountInfo<T>(webDriver);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SkipAccountInfoConfirmation<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            Random rnd = new Random(300);
            Thread.Sleep(rnd.Next(500));
            this.SkipButton(webDriver).Click();

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SignIn<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            SignInButton(webDriver).Click();
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> EnterEmail<T>(IWebDriver webDriver, string email)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                Random sendKeysTime = new Random(200);
                foreach (char character in email)
                {
                    EmailInput(webDriver).SendKeys(character.ToString());

                    // simulates user entering in email
                    Thread.Sleep(sendKeysTime.Next(300));
                }

                // verify email has been entered
                string enteredEmail = EmailInput(webDriver).GetAttribute("value");

                if (enteredEmail != email)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to enter user email into the input field");
                    throw new Exception();
                }
                else
                {
                    _logger.LogInformation("[LinkedInLoginPage]: Successfully verified email field contains the same value as the provided email.");
                }

                Random exitEmailTime = new Random(500);
                Thread.Sleep(exitEmailTime.Next(700));

            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Error occured entering user email.");
                return result;
            }
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> EnterPassword<T>(IWebDriver webDriver, string password)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                Random sendKeysTime = new Random(100);
                foreach (char character in password)
                {
                    PasswordInput(webDriver).SendKeys(character.ToString());

                    // simulates user entering in email
                    Thread.Sleep(sendKeysTime.Next(300));
                }

                // verify email has been entered
                string enteredPassword = PasswordInput(webDriver).GetAttribute("value");

                if (enteredPassword != password)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to enter user password into the input field");
                    throw new Exception();
                }
                else
                {
                    _logger.LogInformation("[LinkedInLoginPage]: Successfully verified password field contains the same value as the provided password.");
                }

                Random exitPasswordTime = new Random(500);
                Thread.Sleep(exitPasswordTime.Next(700));

            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Error occured entering user email.");
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
