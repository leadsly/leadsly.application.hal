using Domain;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PageObjects.Pages
{
    public class LinkedInLoginPage : LeadslyBase, ILinkedInLoginPage
    {
        private const string BaseUrl = "https://www.linkedin.com";

        public LinkedInLoginPage(ILogger<LinkedInLoginPage> logger, IWebDriverUtilities webDriverUtilities, IHumanBehaviorService humanBehaviorService) : base(logger)
        {
            this._logger = logger;

            _webDriverUtilities = webDriverUtilities;
            _humanBehaviorService = humanBehaviorService;
        }
        private readonly ILogger<LinkedInLoginPage> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IWebDriverUtilities _webDriverUtilities;
        private TwoFactorAuthType? _authType = null;
        public TwoFactorAuthType TwoFactorAuthenticationType(IWebDriver webDriver)
        {
            return _authType ??= (TwoFactorAuthAppView(webDriver) != null ? TwoFactorAuthType.AuthenticatorApp : TwoFactorAuthType.SMS);
        }

        public bool SomethingUnexpectedHappenedToastDisplayed(IWebDriver webDriver)
        {
            IWebElement somethingUnexpectedToast = _webDriverUtilities.WaitUntilNotNull(SomethingUnexpectedHappenedToast, webDriver, 5);
            return somethingUnexpectedToast != null;
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

        private IWebElement EmailChallengePinInput(IWebDriver webDriver)
        {
            IWebElement input = null;
            try
            {
                input = EmailChallengePinForm(webDriver).FindElement(By.Id("input__email-verification_pin"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate email challenge pin input control.");
            }
            return input;

        }

        private IWebElement EmailChallengePinSubmitButton(IWebDriver webDriver)
        {

            IWebElement button = null;
            try
            {
                button = EmailChallengePinForm(webDriver).FindElement(By.CssSelector(".form__action button#two-step-submit-button"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate email challenge pin submit button.");
            }
            return button;

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

        private IWebElement EmailChallengePinForm(IWebDriver webDriver)
        {
            IWebElement form = default;
            try
            {
                form = webDriver.FindElement(By.Id("email-pin-challenge"));
            }
            catch (Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Failed to locate email challenge pin form.");
            }

            return form;
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
                _logger.LogDebug("[LinkedInLoginPage]: Failed to locate sign in button.");
            }

            return signInButton;

        }


        public HalOperationResult<T> EnterTwoFactorAuthCode<T>(IWebDriver webDriver, string twoFactorAuthCode)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                foreach (char character in twoFactorAuthCode)
                {
                    TwoFactorAuthenticationCodeInput(webDriver).SendKeys(character.ToString());
                    _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enter two factor auth code");
                result.WebDriverError = true;
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to enter user's two factor auth code",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> EnterEmailChallengePin<T>(IWebDriver driver, string pin)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                foreach (char character in pin)
                {
                    EmailChallengePinInput(driver).SendKeys(character.ToString());
                    _humanBehaviorService.RandomWaitMilliSeconds(500, 850);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enter email challenge pin");
                result.WebDriverError = true;
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to enter user's email challenge pin",
                    Detail = ex.Message
                });
                return result;
            }

            result.Succeeded = true;
            return result;

        }

        public HalOperationResult<T> SubmitTwoFactorAuthCode<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                TwoStepAuthenticationSubmitButton(webDriver).Click();
                _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit user's two factor auth code");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to submit user's two factor auth code",
                    Detail = ex.Message
                });
                result.WebDriverError = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SubmitEmailChallengePin<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                EmailChallengePinSubmitButton(webDriver).Click();
                _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit user's email challenge pin");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to submit user's email challenge pin",
                    Detail = ex.Message
                });
                result.WebDriverError = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> ConfirmAccountInfo<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);
            this.ConfirmButton(webDriver).Click();
            _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);

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

            _humanBehaviorService.RandomWaitMilliSeconds(500, 1000);
            this.SkipButton(webDriver).Click();

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> SignIn<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                IWebElement signInButton = _webDriverUtilities.WaitUntilNotNull(SignInButton, webDriver, 5);
                if (signInButton == null)
                {
                    return result;
                }

                signInButton.Click();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click the sign in button");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to click the sign in button",
                    Detail = ex.Message
                });
                result.WebDriverError = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> EnterEmail<T>(IWebDriver webDriver, string email)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                foreach (char character in email)
                {
                    EmailInput(webDriver).SendKeys(character.ToString());

                    // simulates user entering in email
                    _humanBehaviorService.RandomWaitMilliSeconds(300, 600);
                }

                // verify email has been entered
                string enteredEmail = EmailInput(webDriver).GetAttribute("value");

                if (enteredEmail != email)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to enter user email into the input field");
                    throw new Exception("The entered email by the web driver did not match the email that was specified by the user");
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
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to enter user's email",
                    Detail = ex.Message
                });
                result.WebDriverError = true;
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
                    throw new Exception("The entered password by the web driver did not match the password that was specified by the user");
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
                _logger.LogError(ex, "[LinkedInLoginPage]: Error occured entering user password.");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to enter user's password",
                    Detail = ex.Message
                });
                result.WebDriverError = true;
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement ReAuthEmailInputField(IWebDriver webDriver)
        {
            IWebElement reAuthEmailInputField = default;
            try
            {
                reAuthEmailInputField = webDriver.FindElement(By.Id("username"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to locate email input field.");
            }
            return reAuthEmailInputField;
        }

        private IWebElement ReAuthPasswordInputField(IWebDriver webDriver)
        {
            IWebElement reAuthPasswordInputField = default;
            try
            {
                reAuthPasswordInputField = webDriver.FindElement(By.Id("password"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to locate password input field.");
            }

            return reAuthPasswordInputField;
        }

        public HalOperationResult<T> ReEnterEmail<T>(IWebDriver driver, string email)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                IWebElement reEnterEmailInputField = ReAuthEmailInputField(driver);
                if (reEnterEmailInputField != null)
                {
                    reEnterEmailInputField.Clear();
                    _humanBehaviorService.RandomWaitMilliSeconds(700, 2000);
                    _humanBehaviorService.EnterValues(reEnterEmailInputField, email, 400, 850);
                    string enteredValue = reEnterEmailInputField.GetAttribute("value");
                    if (enteredValue == email)
                    {
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Succeeded = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public HalOperationResult<T> ReEnterPassword<T>(IWebDriver driver, string password)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                IWebElement reEnterPasswordInputField = ReAuthPasswordInputField(driver);
                if (reEnterPasswordInputField != null)
                {
                    reEnterPasswordInputField.Clear();
                    _humanBehaviorService.RandomWaitMilliSeconds(700, 2000);
                    _humanBehaviorService.EnterValues(reEnterPasswordInputField, password, 400, 850);
                    string enteredValue = reEnterPasswordInputField.GetAttribute("value");
                    if (enteredValue == password)
                    {
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Succeeded = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public HalOperationResult<T> ReEnterPasswordIfEmpty<T>(IWebDriver driver, string password)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement reEnterPasswordInputField = ReAuthPasswordInputField(driver);
            if (reEnterPasswordInputField != null)
            {
                string enteredPassword = reEnterPasswordInputField.GetAttribute("value");
                if (enteredPassword == string.Empty)
                {
                    _humanBehaviorService.RandomWaitMilliSeconds(700, 1200);
                    _humanBehaviorService.EnterValues(reEnterPasswordInputField, password, 400, 850);
                    string enteredValue = reEnterPasswordInputField.GetAttribute("value");
                    if (enteredValue == password)
                    {
                        result.Succeeded = true;
                    }
                    else
                    {
                        result.Succeeded = false;
                    }
                }
                else
                {
                    result.Succeeded = true;
                }
            }
            return result;
        }

        private IWebElement ReSignInButton(IWebDriver webDriver)
        {
            IWebElement reSignInButton = default;
            try
            {
                reSignInButton = webDriver.FindElement(By.CssSelector("button[data-litms-control-urn='login-submit']"));
            }
            catch (Exception)
            {
                _logger.LogInformation("Failed to locate re sign in button");
            }
            return reSignInButton;
        }

        public HalOperationResult<T> ReSignIn<T>(IWebDriver driver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                IWebElement reSignInButton = ReSignInButton(driver);
                if (reSignInButton != null)
                {
                    reSignInButton.Click();
                    result.Succeeded = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click the re-sign in button");
            }

            return result;
        }

        public TwoFactorAuthResult DetermineTwoFactorAuthStatus(IWebDriver webDriver)
        {
            TwoFactorAuthResult result = TwoFactorAuthResult.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = TwoFactorAuthViewResult(drv);
                    return result != TwoFactorAuthResult.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        public EmailChallengePinResult DetermineEmailChallengeStatus(IWebDriver webDriver)
        {
            EmailChallengePinResult result = EmailChallengePinResult.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = EmailChallengePinViewResult(drv);
                    return result != EmailChallengePinResult.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        private EmailChallengePinResult EmailChallengePinViewResult(IWebDriver webDriver)
        {
            IWebElement signedIn = _webDriverUtilities.WaitUntilNotNull(HomePageView, webDriver, 3);
            if (signedIn != null)
            {
                return EmailChallengePinResult.SignedIn;
            }

            IWebElement invalidCodeBanner = _webDriverUtilities.WaitUntilNotNull(InvalidEmailChallengePinBanner, webDriver, 3);
            if (invalidCodeBanner != null)
            {
                return EmailChallengePinResult.InvalidOrExpiredPin;
            }

            // wait until twofactor auth
            IWebElement twoFactorAuthAppView = _webDriverUtilities.WaitUntilNotNull(TwoFactorAuthAppView, webDriver, 3);
            if (twoFactorAuthAppView != null)
            {
                return EmailChallengePinResult.TwoFactorAuthRequired;
            }

            IWebElement twoFactorAuthSMSView = _webDriverUtilities.WaitUntilNotNull(TwoFactorAuthSMSView, webDriver, 3);
            if (twoFactorAuthSMSView != null)
            {
                return EmailChallengePinResult.TwoFactorAuthRequired;
            }

            IWebElement errorToast = _webDriverUtilities.WaitUntilNotNull(ErrorToaster, webDriver, 3);
            if (errorToast != null)
            {
                return EmailChallengePinResult.ToastErrorMessage;
            }

            return EmailChallengePinResult.Unknown;
        }

        private TwoFactorAuthResult TwoFactorAuthViewResult(IWebDriver webDriver)
        {
            IWebElement signedIn = _webDriverUtilities.WaitUntilNotNull(HomePageView, webDriver, 3);
            if (signedIn != null)
            {
                return TwoFactorAuthResult.SignedIn;
            }

            IWebElement invalidCodeBanner = _webDriverUtilities.WaitUntilNotNull(InvalidTwoFactorAuthCodeBanner, webDriver, 3);
            if (invalidCodeBanner != null)
            {
                return TwoFactorAuthResult.InvalidOrExpiredCode;
            }

            IWebElement errorToast = _webDriverUtilities.WaitUntilNotNull(ErrorToaster, webDriver, 3);
            if (errorToast != null)
            {
                return TwoFactorAuthResult.ToastErrorMessage;
            }

            return TwoFactorAuthResult.Unknown;
        }

        private IWebElement InvalidTwoFactorAuthCodeBanner(IWebDriver webDriver)
        {
            IWebElement invalidTwoFactorAuthCodeBanner = default;
            try
            {
                invalidTwoFactorAuthCodeBanner = webDriver.FindElement(By.ClassName("body__banner--error"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not locate error banner indicating the two factor auth code was invalid.");
            }
            return invalidTwoFactorAuthCodeBanner;
        }

        private IWebElement InvalidEmailChallengePinBanner(IWebDriver webDriver)
        {
            IWebElement banner = default;
            try
            {
                banner = webDriver.FindElement(By.ClassName("body__banner--error"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not locate error banner indicating the email challenge pin was invalid.");
            }
            return banner;
        }

        private IWebElement ErrorToaster(IWebDriver webDriver)
        {
            IWebElement toast = default;
            try
            {
                toast = webDriver.FindElement(By.CssSelector("artdeco-toast"));
                if (toast.GetAttribute("type") == "error")
                {
                    return toast;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Something unexpected toast displayed. This probably means that the password/code/pin entered is incorrect");
            }
            return toast;
        }
    }
}
