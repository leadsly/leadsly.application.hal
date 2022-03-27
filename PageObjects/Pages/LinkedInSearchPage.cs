using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInSearchPage : ILinkedInSearchPage
    {
        public LinkedInSearchPage(ILogger<LinkedInSearchPage> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LinkedInSearchPage> _logger;

        private IWebElement SearchResultFooter(IWebDriver webDriver)
        {
            IWebElement searchResultFooter = null;
            try
            {
                searchResultFooter = webDriver.FindElement(By.CssSelector(".artdeco-card.mb6"));
                Actions actions = new Actions(webDriver);
                actions.MoveToElement(searchResultFooter);
                actions.Perform();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search result navigational footer");
            }

            return searchResultFooter;
        }

        private IWebElement LastPage(IWebDriver webDriver)
        {
            List<IWebElement> lis = default;
            try
            {
                IWebElement ul = SearchResultFooter(webDriver).FindElement(By.CssSelector(".artdeco-pagination .artdeco-pagination__pages"));
                lis = ul.FindElements(By.TagName("li")).ToList();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate last page in the navigational search result");
            }

            return lis.LastOrDefault();
        }

        private IWebElement SearchResultsContainer(IWebDriver webDriver)
        {
            IWebElement searchResultContainer = default;
            try
            {
                searchResultContainer = webDriver.FindElement(By.CssSelector("#main .search-results-container"));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search results container");
            }

            return searchResultContainer;
        }

        private IWebElement HitList(IWebDriver webDriver)
        {
            IWebElement hitList = default;
            try
            {
                hitList = SearchResultsContainer(webDriver).FindElement(By.CssSelector("div"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search hitlist");
            }

            return hitList;
        }

        private IWebElement ProspectList(IWebDriver webDriver)
        {
            IWebElement propsectListUlNode = default;
            try
            {
                propsectListUlNode = HitList(webDriver).FindElement(By.TagName("ul"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate the propsect list 'ul' node");
            }

            return propsectListUlNode;
        }

        private List<IWebElement> ProspectsAsWebElements(IWebDriver webDriver)
        {
            List<IWebElement> prospects = default;
            try
            {
                prospects = ProspectList(webDriver).FindElements(By.TagName("li")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate prospects as li elements");
            }
            return prospects;
        }

        public HalOperationResult<T> GatherProspects<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            List<IWebElement> prospAsElements = ProspectsAsWebElements(driver);
            if(prospAsElements == null)
            {
                return result;
            }

            IGatherProspects prospects = new GatherProspects
            {
                ProspectElements = prospAsElements
            };

            result.Value = (T)prospects;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement numberOfPages = LastPage(driver);

            if(numberOfPages == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to locate last page element"                    
                });
                return result;
            }

            if(int.TryParse(numberOfPages.Text, out int resultCount) == false)
            {
                return result;
            }

            IGetTotalNumberOfResults numberOfResults = new GetTotalNumberOfResults
            {
                NumberOfResults = resultCount
            };

            result.Value = (T)numberOfResults;
            result.Succeeded = true;
            return result;
        }
    }
}
