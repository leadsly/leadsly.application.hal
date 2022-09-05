using Domain;
using Domain.POMs.Pages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace PageObjects.Pages
{
    public class LinkedInPage : ILinkedInPage
    {
        public LinkedInPage(ILogger<LinkedInPage> logger, IWebDriverUtilities webDriverUtilities)
        {
            this._logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly ILogger<LinkedInPage> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;

        public SignInOperationResult DetermineSignInStatus(IWebDriver webDriver)
        {
            _logger.LogInformation("Determining SignIn status. This is used to determine if user is already authenticated or not");
            SignInOperationResult result = SignInOperationResult.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = SignInViewResult(drv);
                    return result != SignInOperationResult.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        private SignInOperationResult SignInViewResult(IWebDriver webDriver)
        {
            IWebElement homePageNewsFeed = _webDriverUtilities.WaitUntilNotNull(HomePageNewsFeed, webDriver, 3);
            if (homePageNewsFeed != null)
            {
                _logger.LogDebug("SignInOperationResult found HomePage. User is already authenticated");
                return SignInOperationResult.HomePage;
            }

            IWebElement signInContainer = _webDriverUtilities.WaitUntilNotNull(SignInContainer, webDriver, 3);
            if (signInContainer != null)
            {
                _logger.LogDebug("SignInOperationResult found SignInContainer. User is not authenticated");
                return SignInOperationResult.SignIn;
            }

            IWebElement errorForUsernameDiv = _webDriverUtilities.WaitUntilNotNull(ErrorForUsernameDiv, webDriver, 3);
            if (errorForUsernameDiv != null)
            {
                _logger.LogDebug("SignInOperationResult found ErrorForUsernameDiv. User is not authenticated");
                if (errorForUsernameDiv.Displayed)
                {
                    return SignInOperationResult.InvalidEmail;
                }
            }

            IWebElement errorForPasswordDiv = _webDriverUtilities.WaitUntilNotNull(ErrorForPasswordDiv, webDriver, 3);
            if (errorForPasswordDiv != null)
            {
                _logger.LogDebug("SignInOperationResult found ErrorForPasswordDiv. User is not authenticated");
                if (errorForPasswordDiv.Displayed)
                {
                    return SignInOperationResult.InvalidPassword;
                }
            }

            return SignInOperationResult.Unknown;
        }

        public AfterSignInResult DetermineAfterSigninStatus(IWebDriver webDriver)
        {
            _logger.LogInformation("Determining status of the page after hal clicked to sign user in");
            AfterSignInResult result = AfterSignInResult.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = AfterSignInViewResult(drv);
                    return result != AfterSignInResult.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        private AfterSignInResult AfterSignInViewResult(IWebDriver webDriver)
        {
            IWebElement newPlaceSecurityCodeContainer = _webDriverUtilities.WaitUntilNotNull(EmailPinChallengeForm, webDriver, 3);
            if (newPlaceSecurityCodeContainer != null)
            {
                _logger.LogDebug("AfterSignInResult found EmailPinChallengeForm. User is not authenticated");
                return AfterSignInResult.EmailPinChallenge;
            }

            IWebElement twoFactorAuthContainer = _webDriverUtilities.WaitUntilNotNull(TwoFactorAuthContainer, webDriver, 3);
            if (twoFactorAuthContainer != null)
            {
                _logger.LogDebug("AfterSignInResult found TwoFactorAuthContainer. User is not authenticated");
                return AfterSignInResult.TwoFactorAuthRequired;
            }

            IWebElement homePageNewsFeed = _webDriverUtilities.WaitUntilNotNull(HomePageNewsFeed, webDriver, 3);
            if (homePageNewsFeed != null)
            {
                _logger.LogDebug("AfterSignInResult found HomePage. User is authenticated");
                return AfterSignInResult.HomePage;
            }

            IWebElement errorForUsernameDiv = _webDriverUtilities.WaitUntilNotNull(ErrorForUsernameDiv, webDriver, 3);
            if (errorForUsernameDiv != null)
            {
                _logger.LogDebug("AfterSignInResult found ErrorForUsernameDiv. User is not authenticated");
                if (errorForUsernameDiv.Displayed)
                {
                    return AfterSignInResult.InvalidEmail;
                }
            }

            IWebElement errorForPasswordDiv = _webDriverUtilities.WaitUntilNotNull(ErrorForPasswordDiv, webDriver, 3);
            if (errorForPasswordDiv != null)
            {
                _logger.LogDebug("AfterSignInResult found ErrorForPasswordDiv. User is not authenticated");
                if (errorForPasswordDiv.Displayed)
                {
                    return AfterSignInResult.InvalidPassword;
                }
            }

            IWebElement errorToast = _webDriverUtilities.WaitUntilNotNull(ErrorToaster, webDriver, 3);
            if (errorToast != null)
            {
                _logger.LogDebug("AfterSignInResult found ErrorToaster. User is not authenticated");
                return AfterSignInResult.ToastMessageError;
            }

            return AfterSignInResult.Unknown;
        }


        public IWebElement ErrorToaster(IWebDriver webDriver)
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
                _logger.LogInformation("Something unexpected toast displayed. This probably means that the password entered is incorrect");
            }
            return toast;
        }

        public IWebElement ErrorForUsernameDiv(IWebDriver webDriver)
        {
            IWebElement errorForUsernameDiv = default;
            try
            {
                IWebElement credentialsContainer = CredentialsContainer(webDriver);
                if (credentialsContainer != null)
                {
                    errorForUsernameDiv = credentialsContainer.FindElement(By.Id("error-for-username"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Couldnt find 'error-for-username' div");
            }
            return errorForUsernameDiv;
        }

        public IWebElement ErrorForPasswordDiv(IWebDriver webDriver)
        {
            IWebElement errorForPasswordDiv = default;
            try
            {
                IWebElement credentialsContainer = CredentialsContainer(webDriver);
                if (credentialsContainer != null)
                {
                    errorForPasswordDiv = credentialsContainer.FindElement(By.Id("error-for-password"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Couldnt find 'error-for-password' div");
            }
            return errorForPasswordDiv;
        }

        public IWebElement CredentialsContainer(IWebDriver webDriver)
        {
            IWebElement credentialsContainer = default;
            try
            {
                credentialsContainer = webDriver.FindElement(By.ClassName("login__form"));
            }
            catch (Exception)
            {
                _logger.LogInformation("Failed to locate credentials container");
            }
            return credentialsContainer;
        }

        public IWebElement TwoFactorAuthContainer(IWebDriver webDriver)
        {
            IWebElement twoFactorAuthContainer = default;
            twoFactorAuthContainer = TwoFactorAuthAppView(webDriver);
            if (twoFactorAuthContainer != null)
            {
                return twoFactorAuthContainer;
            }
            twoFactorAuthContainer = TwoFactorAuthSMSView(webDriver);
            if (twoFactorAuthContainer != null)
            {
                return twoFactorAuthContainer;
            }

            return twoFactorAuthContainer;
        }

        public IWebElement EmailPinChallengeForm(IWebDriver webDriver)
        {
            IWebElement emailPinChallengeFormView = EmailPinChallengeFormView(webDriver);

            return emailPinChallengeFormView;
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
                _logger.LogInformation("[LinkedInLoginPage]: Failed to locate two factor auth sms view.");
            }

            return twoFactorAuthSMSView;
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
                _logger.LogInformation("[LinkedInLoginPage]: Failed to locate two factor auth app view.");
            }

            return twoFactorAuthAppView;
        }

        private IWebElement EmailPinChallengeFormView(IWebDriver webDriver)
        {
            IWebElement emailPinChallengeForm = default;
            try
            {
                emailPinChallengeForm = webDriver.FindElement(By.Id("email-pin-challenge"));
                if (emailPinChallengeForm != null)
                {
                    _logger.LogDebug("Email pin challenge form was found");
                    // in case the form is not displayed or visible but is found
                    if (emailPinChallengeForm.Displayed)
                    {
                        _logger.LogDebug("Email pin challenge form is displayed");
                        return emailPinChallengeForm;
                    }
                    else
                    {
                        _logger.LogDebug("Email pin challenge form is not displayed");
                        emailPinChallengeForm = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("[LinkedInLoginPage]: Failed to locate Email Pin Challenge form.");
            }

            return emailPinChallengeForm;
        }

        private IWebElement SignInContainer(IWebDriver webDriver)
        {
            IWebElement signInContainer = null;
            try
            {
                signInContainer = webDriver.FindElement(By.ClassName("sign-in-form-container"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate SignInContainer by class name 'sign-in-form-container'");
            }
            return signInContainer;
        }

        private IWebElement HomePageNewsFeed(IWebDriver webDriver)
        {
            IWebElement newsFeed = default;
            try
            {
                newsFeed = webDriver.FindElement(By.Id("voyager-feed"));
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Voyagers news feed not found");
            }

            return newsFeed;
        }

        public void NavigateToPage(IWebDriver webDriver, string pageUrl)
        {

            try
            {
                webDriver.Navigate().GoToUrl(new Uri(pageUrl));
            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                throw timeoutEx;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to navigate to page {pageUrl}");
                throw ex;
            }
        }
    }
}
