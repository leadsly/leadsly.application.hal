using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private readonly IWebDriver _driver;

        public void GoToPage()
        {
            this._driver.Manage().Window.Maximize();
            this._driver.Navigate().GoToUrl(new Uri(BaseUrl));            
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
