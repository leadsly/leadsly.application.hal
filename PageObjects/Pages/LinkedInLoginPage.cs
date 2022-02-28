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
    public class LinkedInLoginPage : LeadslyBasePage
    {
        private const string BaseUrl = "https://www.linkedin.com";
        
        public LinkedInLoginPage(IWebDriver driver, ILogger logger)
        {
            this._driver = driver;
            this._logger = logger;
            this._wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;
        private readonly IWebDriver _driver;
        private TwoFactorAuthType? _authType = null;
        public TwoFactorAuthType TwoFactorAuthenticationType
        {
            get
            {
                return _authType ??= (TwoFactorAuthAppView != null ? TwoFactorAuthType.AuthenticatorApp : TwoFactorAuthType.SMS);
            }
        }

        public bool SomethingUnexpectedHappenedToastDisplayed 
        {
            get
            {
                return SomethingUnexpectedHappenedToast != null;
            }
        }        

        public IWebElement SomethingUnexpectedHappenedToast
        {
            get
            {
                IWebElement toast = null;
                try
                {
                    toast = _driver.FindElement(By.CssSelector("artdeco-toasts"));    
                    if(toast.GetAttribute("type") == "error")
                    {
                        return toast;
                    }
                    return null;
                }
                catch(Exception ex)
                {

                }
                return toast;
            }
        }

        public bool CheckIfUnexpectedViewRendered
        {
            get
            {
                if(HomePageView == null)
                {
                    if(TwoFactorAuthAppView == null && TwoFactorAuthSMSView == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public IWebElement HomePageView 
        {
            get
            {
                IWebElement homePageView = null;
                try
                {
                    homePageView = _driver.FindElement(By.Id("voyager-feed"));
                }
                catch(Exception ex)
                {

                }
                return homePageView;
            }
        }

        public bool IsTwoFactorAuthRequired
        {
            get
            {
                if (TwoFactorAuthAppView != null)
                {
                    _authType = TwoFactorAuthType.AuthenticatorApp;
                    return true;
                }
                else if(TwoFactorAuthSMSView != null)
                {
                    _authType = TwoFactorAuthType.SMS;
                    return true;
                }
                _authType = TwoFactorAuthType.None;
                return false;            
            }
        }

        public IWebElement TwoFactorAuthenticationCodeInput
        {
            get
            {
                IWebElement twoFactorAuthenticationCodeInput = null;
                try
                {
                    if (TwoFactorAuthenticationType == TwoFactorAuthType.AuthenticatorApp)
                    {
                        twoFactorAuthenticationCodeInput = _wait.Until(drv => TwoFactorAuthAppView.FindElement(By.Id("input__phone_verification_pin")));
                    }
                    else
                    {
                        twoFactorAuthenticationCodeInput = _wait.Until(drv => TwoFactorAuthSMSView.FindElement(By.Id("input__phone_verification_pin")));
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor authentication input control.");
                }
                return twoFactorAuthenticationCodeInput;
            }
        }

        private IWebElement TwoStepAuthenticationSubmitButton
        {
            get
            {
                IWebElement twoFactorSubmitButton = null;
                try
                {
                    if(TwoFactorAuthenticationType == TwoFactorAuthType.AuthenticatorApp)
                    {
                        twoFactorSubmitButton = _wait.Until(drv => TwoFactorAuthAppView.FindElement(By.CssSelector("#auth-app-div button#two-step-submit-button")));
                    }
                    else
                    {
                        twoFactorSubmitButton = _wait.Until(drv => TwoFactorAuthSMSView.FindElement(By.CssSelector("#app__container button#two-step-submit-button")));
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate two step authentication submit button.");
                }
                return twoFactorSubmitButton;
            }
        }

        private IWebElement TwoFactorAuthAppView
        {
            get
            {
                IWebElement twoFactorAuthAppView = default;
                try
                {
                    twoFactorAuthAppView = _driver.FindElement(By.Id("auth-app-div"));
                }
                catch (Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor auth app view.");
                }

                return twoFactorAuthAppView;
            }
        }

        private IWebElement TwoFactorAuthSMSView
        {
            get
            {
                IWebElement twoFactorAuthSMSView = default;
                try
                {
                    twoFactorAuthSMSView = _driver.FindElement(By.Id("app__container"));
                    try
                    {
                        // verify this is two factor auth id=input__phone_verification_pin 
                        IWebElement sms2faInput = twoFactorAuthSMSView.FindElement(By.Id("input__phone_verification_pin"));                        
                        if(sms2faInput == null)
                        {
                            twoFactorAuthSMSView = null;
                        }
                    }
                    catch(Exception ex)
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
        }


        public bool ConfirmAccountDisplayed
        {
            get
            {
                string currentUrl = this._driver.Url;
                if (currentUrl.Contains("check/manage-account"))
                {
                    // most likely we need to confirm our account before proceeding
                    if (ConfirmButton.Displayed)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool DidVerificationCodeFailed 
        {
            get
            {
                return VerificationCodeFailureBanner != null;
            }
        }

        private IWebElement VerificationCodeFailureBanner 
        {
            get
            {
                IWebElement verificationCodeFailureBanner = null;
                try
                {
                    verificationCodeFailureBanner = _wait.Until(drv => drv.FindElement(By.CssSelector("span[role='alert']")));
                }
                catch(Exception ex)
                {
                    _logger.LogWarning("[LinkedInLoginPage]: Failed to locate two factor authentication error banner. This is most likely desired behavior.");
                }
                return verificationCodeFailureBanner;
            }
        }

        public bool SMSVerificationCodeErrorDisplayed
        {
            get
            {
                return SMSVerificationCodeError != null;
            }
        }

        private IWebElement SMSVerificationCodeError
        {
            get
            {
                IWebElement smsVerificationCodeError = default;
                try
                {
                    smsVerificationCodeError = _driver.FindElement(By.CssSelector(".app__content .body__banner-wrapper .body__banner--error span"));
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
        }

        private IWebElement SkipButton
        {
            get
            {
                IWebElement skipButton = null;
                IEnumerable<IWebElement> skipButtons = _wait.Until(drv => drv.FindElements(By.CssSelector("button[type='button']")));
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
        }

        private IWebElement ConfirmButton
        {
            get
            {
                IWebElement submitButton = null;
                IEnumerable<IWebElement> submitButtons = _wait.Until(drv => drv.FindElements(By.CssSelector("button[type='submit']")));
                foreach (IWebElement submitBttn in submitButtons)
                {
                    if(submitBttn.Text == "Confirm")
                    {
                        submitButton = submitBttn;
                        break;
                    }
                }
                return submitButton;
            }
        }

        private IWebElement LoginContainer
        {
            get
            {
                IWebElement loginContainer = default;
                try
                {
                    loginContainer = _wait.Until(drv => drv.FindElement(By.CssSelector(".sign-in-form-container")));
                }
                catch(Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate loginContainer.");
                }

                return loginContainer;
            }
        }

        private IEnumerable<IWebElement> CredentialInputs
        {
            get
            {
                IEnumerable<IWebElement> inputs = default;
                try
                {
                    inputs = _wait.Until(drv => LoginContainer?.FindElements(By.CssSelector(".sign-in-form__form-input-container .input__input")) ?? throw new Exception());
                }
                catch (Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate credential inputs.");
                }

                return inputs;
            }
        }

        private IWebElement EmailInput
        {
            get
            {
                IWebElement emailInput = null;
                try
                {
                    foreach (IWebElement input in CredentialInputs)
                    {
                        if(input.GetAttribute("autocomplete") == "username")
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
        }

        private IWebElement PasswordInput
        {
            get
            {
                IWebElement passwordInput = null;
                try
                {
                    foreach (IWebElement input in CredentialInputs)
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
        }

        private IWebElement SignInButton
        {
            get
            {
                IWebElement signInButton = null;
                try
                {
                    signInButton = _wait.Until(drv => drv.FindElement(By.CssSelector(".sign-in-form__submit-button")));
                    _wait.Until(drv => signInButton.Displayed && signInButton.Enabled);                    
                }
                catch(Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate sign in button.");
                }

                return signInButton;
            }
        }
        
        public void EnterTwoFactorAuthCode(string twoFactorAuthCode)
        {
            Random rnd = new Random(300);
            foreach (char character in twoFactorAuthCode)
            {
                TwoFactorAuthenticationCodeInput.SendKeys(character.ToString());
                Thread.Sleep(rnd.Next(500));
            }            
        }

        public void SubmitTwoFactorAuthCode()
        {
            Random rnd = new Random(300);
            TwoStepAuthenticationSubmitButton.Click();
            Thread.Sleep(rnd.Next(1000));
        }

        public void ConfirmAccountInfo()
        {
            Random rnd = new Random(300);
            Thread.Sleep(rnd.Next(500));
            this.ConfirmButton.Click();
            Thread.Sleep(rnd.Next(500));

            if (this.ConfirmAccountDisplayed)
            {
                ConfirmAccountInfo();
            }
            
        }

        public void SkipAccountInfoConfirmation()
        {
            Random rnd = new Random(300);
            Thread.Sleep(rnd.Next(500));
            this.SkipButton.Click();
        }

        public void SignIn()
        {
            SignInButton.Click();
        }

        public void EnterEmail(string email) 
        {
            try
            {
                Random sendKeysTime = new Random(200);
                foreach (char character in email)
                {
                    EmailInput.SendKeys(character.ToString());

                    // simulates user entering in email
                    Thread.Sleep(sendKeysTime.Next(300));
                }                

                // verify email has been entered
                string enteredEmail = EmailInput.GetAttribute("value");

                if(enteredEmail != email)
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
            catch(Exception ex)
            {
                _logger.LogError("[LinkedInLoginPage]: Error occured entering user email.");
            }            
        }

        public void EnterPassword(string password)
        {
            try
            {
                Random sendKeysTime = new Random(100);
                foreach (char character in password)
                {
                    PasswordInput.SendKeys(character.ToString());

                    // simulates user entering in email
                    Thread.Sleep(sendKeysTime.Next(300));
                }

                // verify email has been entered
                string enteredPassword = PasswordInput.GetAttribute("value");

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
            }
        }
    }
}
