using OpenQA.Selenium;

namespace Domain.Services.Interfaces.POMs
{
    public interface ISearchPageFooterServicePOM
    {
        public int? GetTotalResults(IWebDriver webDriver, bool scrollTop = false);
        public bool? IsLastPage(IWebDriver webDriver, bool verifyWithWebDriver = false, int? currentPage = null, int? totalResultCount = null);
        public bool? GoToTheNextPage(IWebDriver webDriver);
        public bool? GoToThePreviousPage(IWebDriver webDriver);
        public bool? IsPreviousButtonClickable(IWebDriver webDriver);
        public bool? IsNextButtonClickable(IWebDriver webDriver);
    }
}
