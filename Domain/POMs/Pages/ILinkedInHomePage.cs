using OpenQA.Selenium;

namespace Domain.POMs.Pages
{
    public interface ILinkedInHomePage
    {
        bool IsNewsFeedDisplayed(IWebDriver webDriver);

        void WaitUntilNewsFeedIsDisplayed(IWebDriver webDriver);

    }
}
