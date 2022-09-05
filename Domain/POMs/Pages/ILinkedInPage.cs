using OpenQA.Selenium;

namespace Domain.POMs.Pages
{
    public interface ILinkedInPage
    {
        void NavigateToPage(IWebDriver webDriver, string pageUrl);

        SignInOperationResult DetermineSignInStatus(IWebDriver webDriver);

        AfterSignInResult DetermineAfterSigninStatus(IWebDriver webDriver);
    }
}
