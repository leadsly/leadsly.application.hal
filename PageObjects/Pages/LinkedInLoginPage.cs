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
                    homePageView = _wait.Until(drv => drv.FindElement(By.Id("voyager-feed")));
                }
                catch(Exception ex)
                {

                }
                return homePageView;
            }
        }


        //public bool SomethingUnexpectedOccured 
        //{
        //    get
        //    {
        //        return SomethingUnexpectedToast != null || EmailInputUnExpected != null;
        //    }
        //}

        //private IWebElement SomethingUnexpectedToast
        //{
        //    get
        //    {
        //        IWebElement somethingUnexpectedToast = null;
        //        try
        //        {
        //            somethingUnexpectedToast = _wait.Until(drv => drv.FindElement(By.ClassName("artdeco-toast-inner")));
        //        }
        //        catch(Exception ex)
        //        {

        //        }
        //        return somethingUnexpectedToast;
        //    }
        //}

        //private IWebElement EmailInputUnExpected
        //{
        //    get
        //    {
        //        IWebElement emailInputUnExpected = null;
        //        try
        //        {
        //            emailInputUnExpected = _wait.Until(drv => drv.FindElement(By.Id("username")));
        //        }
        //        catch(Exception ex)
        //        {

        //        }
        //        return emailInputUnExpected;

        //    }
        //}

        public bool IsTwoFactorAuthRequired
        {
            get
            {
                if(TwoFactorAuthAppView != null)
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
                    twoFactorAuthAppView = _wait.Until(drv => drv.FindElement(By.Id("auth-app-div")));
                }
                catch (Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor auth app view.");
                }

                return twoFactorAuthAppView;
            }
        }

        private IWebElement _twoFactorAuthAppView;

        private IWebElement TwoFactorAuthSMSView
        {
            get
            {
                IWebElement twoFactorAuthSMSView = default;
                try
                {
                    twoFactorAuthSMSView = _wait.Until(drv => drv.FindElement(By.Id("app__container")));
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

        private IWebElement TwoFactorAuthEnterCodeHeader
        {
            get
            {
                IWebElement twoFactorAuthEnterCodeHeader = null;
                try
                {
                    if(TwoFactorAuthenticationType == TwoFactorAuthType.AuthenticatorApp)
                    {
                        twoFactorAuthEnterCodeHeader = _wait.Until(drv => TwoFactorAuthAppView.FindElement(By.CssSelector("#auth-app-div .content__header")));
                    }
                    else
                    {
                        twoFactorAuthEnterCodeHeader = _wait.Until(drv => TwoFactorAuthSMSView.FindElement(By.CssSelector(".content__header")));
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError("[LinkedInLoginPage]: Failed to locate two factor authentication header. This text contains something like 'Enter the code you see...'");
                }
                return twoFactorAuthEnterCodeHeader;
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
        
        public void GoToPage()
        {
            this._driver.Manage().Window.Maximize();
            this._driver.Navigate().GoToUrl(new Uri(BaseUrl));            
        }

        //public bool AfterTwoFactorSubmitErrorDisplayed
        //{
        //    get
        //    {
        //        if(AfterTwoFactorSubmitErrorView != null)
        //        {
        //            _logger.LogWarning("[LinkedInLoginPage] LinkedIn displayed an error page which will automatically refresh and take us back to the login page. Sit on the page until this page is gone and login page is displayed again.");
        //            return true;
        //        }
        //        else
        //        {
        //            // check for specific text
        //            IWebElement errorHeader = _driver.FindElement(By.CssSelector("#main h1"));
        //            IEnumerable<IWebElement> errorParagraphs = _driver.FindElements(By.CssSelector("#main p"));
        //            IList<bool> errorPageElementsFound = new List<bool>();
        //            if (errorHeader.Text.Contains("Your LinkedIn Network Will Be Back Soon"))
        //            {
        //                _logger.LogWarning("[LinkedInLoginPage] LinkedIn might have displayed a potential error page on which we have to wait to get back to login screen again. Keep checking for known elements.");
        //                errorPageElementsFound.Add(true);
        //            }
        //            foreach (IWebElement errorParagraph in errorParagraphs)
        //            {
        //                if (errorParagraph.Text.Contains("We’ll get you reconnected soon"))
        //                {
        //                    errorPageElementsFound.Add(true);
        //                }
        //                if (errorParagraph.Text.Contains("You can leave this window open"))
        //                {
        //                    errorPageElementsFound.Add(true);
        //                }
        //                if(errorParagraph.Text.Contains("We apologize for the interruption"))
        //                {
        //                    errorPageElementsFound.Add(true);
        //                }
        //            }

        //            if(errorPageElementsFound.Any(e => e == true))
        //            {
        //                _logger.LogWarning("[LinkedInLoginPage] LinkedIn displayed an error page which will automatically refresh and take us back to the login page. Sit on the page until this page is gone and login page is displayed again.");
        //                return true;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }                
        //    }
        //}

        private IWebElement AfterTwoFactorSubmitErrorView
        {
            get
            {
                IWebElement afterTwoFactorSubmitErrorView = null;
                try
                {
                    afterTwoFactorSubmitErrorView = _driver.FindElement(By.CssSelector(".errorpg"));
                }
                catch(Exception ex)
                {

                }
                return afterTwoFactorSubmitErrorView;
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
