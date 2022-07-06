using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;

namespace Domain.POMs.Pages
{
    public interface ILinkedInPage
    {
        public bool IsAuthenticationRequired(IWebDriver webDriver);
        public bool IsSignInContainerDisplayed(IWebDriver webDriver);
        HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse;

        void NavigateToPage(IWebDriver webDriver, string pageUrl);

        SignInOperationResult DetermineSignInStatus(IWebDriver webDriver);

        AfterSignInResult DetermineAfterSigninStatus(IWebDriver webDriver);
    }
}
