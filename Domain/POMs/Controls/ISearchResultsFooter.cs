using OpenQA.Selenium;

namespace Domain.POMs.Controls
{
    public interface ISearchResultsFooter
    {
        int? GetTotalSearchResults(IWebDriver webDriver);
        IWebElement LinkInFooterLogoIcon(IWebDriver webDriver);
        bool? IsNextButtonClickable(IWebDriver webDriver);
        bool? ClickNext(IWebDriver webDriver);
        bool? IsPreviousButtonClickable(IWebDriver webDriver);
        bool? ClickPrevious(IWebDriver webDriver);
    }
}
