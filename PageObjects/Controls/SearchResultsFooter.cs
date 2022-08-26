using Domain.POMs.Controls;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Controls
{
    public class SearchResultsFooter : ISearchResultsFooter
    {
        public SearchResultsFooter(ILogger<SearchResultsFooter> logger, IWebDriverUtilities webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly ILogger<SearchResultsFooter> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;

        public bool? ClickNext(IWebDriver webDriver)
        {
            bool? succeeded = false;
            IWebElement nextBtn = _webDriverUtilities.WaitUntilNotNull(NextButton, webDriver, 10);
            if (nextBtn == null)
            {
                _logger.LogWarning("[ClickNext]: Next button could not be located");
                succeeded = null;
            }

            try
            {
                _logger.LogTrace("Clicking next button");
                nextBtn.Click();
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClickNext]: Failed to click next button");
                succeeded = false;
            }

            return succeeded;
        }

        public bool? ClickPrevious(IWebDriver webDriver)
        {
            bool? succeeded = false;
            IWebElement previousBtn = _webDriverUtilities.WaitUntilNotNull(PreviousButton, webDriver, 10);
            if (previousBtn == null)
            {
                _logger.LogWarning("[ClickPrevious]: Previous button could not be located");
                succeeded = null;
            }

            try
            {
                _logger.LogTrace("Clicking previous button");
                previousBtn.Click();
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClickPrevious]: Failed to click previous button");
                succeeded = false;
            }

            return succeeded;
        }

        public int? GetTotalSearchResults(IWebDriver webDriver)
        {
            IWebElement numberOfPagesLi = LastPage(webDriver);

            if (numberOfPagesLi == null)
            {
                _logger.LogWarning("Could not determine the number of pages in the hitlist");
                return null;
            }

            if (int.TryParse(numberOfPagesLi.Text, out int resultCount) == false)
            {
                return null;
            }

            return resultCount;
        }

        public bool? IsNextButtonClickable(IWebDriver webDriver)
        {
            IWebElement nextButton = _webDriverUtilities.WaitUntilNotNull(NextButton, webDriver, 5);

            if (nextButton == null)
            {
                return null;
            }

            string disabledAttribute = nextButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            // if the button does not contain disabled attribute, the return value will be null. out bool will produce false. So we want to flip the condition
            return !result;
        }

        public bool? IsPreviousButtonClickable(IWebDriver webDriver)
        {
            IWebElement previousButton = _webDriverUtilities.WaitUntilNotNull(PreviousButton, webDriver, 5);

            if (previousButton == null)
            {
                return null;
            }

            string disabledAttribute = previousButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            // if the button does not contain disabled attribute, the return value will be null. out bool will produce false. So we want to flip the condition
            return !result;
        }

        public IWebElement LinkInFooterLogoIcon(IWebDriver webDriver)
        {
            IWebElement logo = _webDriverUtilities.WaitUntilNotNull(LinkedInLogoFooter, webDriver, 10);

            return logo;
        }

        private IWebElement NextButton(IWebDriver webDriver)
        {
            IWebElement nextBtn = default;
            try
            {
                nextBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Next']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[NextButton] Failed to find next button");
            }

            return nextBtn;
        }

        private IWebElement LastPage(IWebDriver webDriver)
        {
            List<IWebElement> lis = default;
            try
            {
                _logger.LogTrace("Getting last page of the search results from the hitlist");
                IWebElement searchResultsFooter = _webDriverUtilities.WaitUntilNotNull(SearchResultFooter, webDriver, 10);
                if (searchResultsFooter == null)
                {
                    return null;
                }

                IWebElement ul = SearchResultsPaginatorDiv(searchResultsFooter);
                if (ul == null)
                {
                    return null;
                }

                lis = ul.FindElements(By.TagName("li")).ToList();
                string last = lis.LastOrDefault()?.Text;
                _logger.LogTrace("Successfully found total hit list page result count. Last page is: {last}", last);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate last page in the navigational search result");
            }

            return lis?.LastOrDefault();
        }

        private IWebElement LinkedInLogoFooter(IWebDriver webDriver)
        {
            IWebElement logo = default;
            try
            {
                logo = webDriver.FindElement(By.CssSelector("footer li-icon[aria-label='LinkedIn']"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to locate LinkedIn logo in the footer");
            }
            return logo;
        }

        private IWebElement SearchResultsPaginatorDiv(IWebElement searchResultsFooter)
        {
            IWebElement searchResultsFooterUl = default;
            try
            {
                searchResultsFooterUl = searchResultsFooter.FindElement(By.CssSelector(".artdeco-pagination .artdeco-pagination__pages"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate SearchResultsFooter ul element");
            }
            return searchResultsFooterUl;
        }

        private IWebElement SearchResultFooter(IWebDriver webDriver)
        {
            IWebElement searchResultFooter = null;
            try
            {
                _logger.LogInformation("Retrieving search results footer and then trying to move it into view with 'MoveToElement' method");
                searchResultFooter = webDriver.FindElement(By.XPath("//ul[contains(@class, 'artdeco-pagination__pages--number')]/parent::div/parent::div"));
                Actions actions = new Actions(webDriver);
                actions.MoveToElement(searchResultFooter);
                actions.Perform();
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate search result navigational footer");
            }

            return searchResultFooter;
        }

        private IWebElement PreviousButton(IWebDriver webDriver)
        {
            IWebElement previousBtn = default;
            try
            {
                previousBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Previous']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[PreviousButton] Failed to find previous button");
            }

            return previousBtn;
        }
    }
}
