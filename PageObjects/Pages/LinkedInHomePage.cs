using Domain.POMs.Pages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace PageObjects.Pages
{
    public class LinkedInHomePage : ILinkedInHomePage
    {
        public LinkedInHomePage(ILogger<LinkedInHomePage> logger, IWebDriverUtilities webDriverUtilities)
        {
            this._logger = logger;
            _webDriverUtilities = webDriverUtilities;

        }

        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly ILogger<LinkedInHomePage> _logger;

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

        public bool IsNewsFeedDisplayed(IWebDriver webDriver)
        {
            _logger.LogDebug("Checking to see if linked in news feed is displayed. Waiting for 30 seconds");
            IWebElement voyagersNewsFeed = _webDriverUtilities.WaitUntilNotNull(HomePageNewsFeed, webDriver, 30);
            _logger.LogDebug($"LinkedIn news feed is {(voyagersNewsFeed == null ? "not" : "")} displayed");

            return voyagersNewsFeed != null;
        }

        public void WaitUntilNewsFeedIsDisplayed(IWebDriver webDriver)
        {
            _webDriverUtilities.WaitUntilNotNull(HomePageNewsFeed, webDriver, 60);
        }
    }
}
