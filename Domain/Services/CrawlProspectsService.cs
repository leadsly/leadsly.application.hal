using Domain.Facades.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services
{
    public class CrawlProspectsService : ICrawlProspectsService
    {
        public CrawlProspectsService(ILogger<CrawlProspectsService> logger, IHumanBehaviorService humanBehaviorService, ILinkedInPageFacade linkedInPageFacade)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _linkedInPageFacade = linkedInPageFacade;
        }

        private readonly ILogger<CrawlProspectsService> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;

        public bool CrawlProspects(IWebDriver webDriver, string primaryProspectListId, out IList<IWebElement> rawCollectedProspects)
        {
            bool crawlResult = false;
            rawCollectedProspects = new List<IWebElement>();

            bool isNoSearchResultsContainerDisplayed = _linkedInPageFacade.LinkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
            if (isNoSearchResultsContainerDisplayed == true)
            {
                HalOperationResult<IOperationResponse> retrySearchResult = _linkedInPageFacade.LinkedInSearchPage.ClickRetrySearch<IOperationResponse>(webDriver);
                if (retrySearchResult.Succeeded == false)
                {
                    return crawlResult;
                }
            }

            _humanBehaviorService.RandomWaitMilliSeconds(2000, 7000);
            IWebElement resultsDiv = _linkedInPageFacade.LinkedInSearchPage.ResultsHeader(webDriver);
            _humanBehaviorService.RandomClickElement(resultsDiv);

            HalOperationResult<IGatherProspects> result = _linkedInPageFacade.LinkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
            // ify on this, perhpas this should just return false
            if (result.Succeeded == false)
            {
                crawlResult = true;
                return crawlResult;
            }

            rawCollectedProspects = result.Value.ProspectElements;
            List<IWebElement> propsAsWebElements = result.Value.ProspectElements;

            // we need to perform a random scroll
            if (propsAsWebElements.Count > 3)
            {
                _linkedInPageFacade.LinkedInSearchPage.ScrollIntoView(propsAsWebElements[2], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            if (propsAsWebElements.Count > 8)
            {
                _linkedInPageFacade.LinkedInSearchPage.ScrollIntoView(propsAsWebElements[7], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            _linkedInPageFacade.LinkedInSearchPage.ScrollTop(webDriver);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 3000);

            _logger.LogTrace("Creating PrimaryProspects from IWebElements");

            IWebElement areResultsHelpfulText = _linkedInPageFacade.LinkedInSearchPage.AreResultsHelpfulPTag(webDriver);
            _humanBehaviorService.RandomClickElement(areResultsHelpfulText);

            crawlResult = true;
            return crawlResult;
        }

        public IList<PrimaryProspectRequest> CreatePrimaryProspects(IList<IWebElement> prospects, string primaryProspectListId)
        {
            IList<PrimaryProspectRequest> primaryProspects = new List<PrimaryProspectRequest>();

            foreach (IWebElement webElement in prospects)
            {
                primaryProspects.Add(new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = GetProspectsName(webElement),
                    ProfileUrl = GetProspectsProfileUrl(webElement),
                    SearchResultAvatarUrl = GetProspectsSearchResultAvatarUrl(webElement),
                    Area = GetProspectsArea(webElement),
                    PrimaryProspectListId = primaryProspectListId,
                    EmploymentInfo = GetProspectsEmploymentInfo(webElement)
                });
            }

            return primaryProspects;
        }

        private string GetProspectsName(IWebElement webElement)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement prospectNameSpan = webElement.FindElement(By.CssSelector(".entity-result__title-text"));
                try
                {
                    IWebElement thirdConnectionProspectName = prospectNameSpan.FindElement(By.CssSelector("span[aria-hidden=true]"));
                    prospectName = thirdConnectionProspectName.Text;
                }
                catch (Exception ex)
                {
                    // ignore the error and proceed
                    prospectName = prospectNameSpan.Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects name");
            }

            return prospectName;
        }
        private string GetProspectsProfileUrl(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            string userName = innerText[0] ?? string.Empty;
            if (userName == "LinkedIn Member")
            {
                // this means we don't have access to user's profile
                return string.Empty;
            }

            string profileUrl = string.Empty;
            try
            {
                IWebElement anchorTag = webElement.FindElement(By.CssSelector(".app-aware-link"));
                profileUrl = anchorTag.GetAttribute("href");
                profileUrl = profileUrl.Split('?').FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects profile url");
            }

            return profileUrl;
        }
        private string GetProspectsSearchResultAvatarUrl(IWebElement webElement)
        {
            string avatarSrc = string.Empty;
            try
            {
                IWebElement img = webElement.FindElement(By.CssSelector(".presence-entity")).FindElement(By.TagName("img"));
                avatarSrc = img.GetAttribute("src");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not extract user's avatar. This probably means user does not have an avatar set.");
            }
            return avatarSrc;
        }
        private string GetProspectsArea(IWebElement webElement)
        {
            string prospectArea = string.Empty;
            try
            {
                IWebElement secondarySubTitleElement = webElement.FindElement(By.CssSelector(".entity-result__secondary-subtitle"));
                prospectArea = secondarySubTitleElement.Text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects area");
            }
            return prospectArea;
        }
        private string GetProspectsEmploymentInfo(IWebElement webElement)
        {
            string prospectEmploymentInfo = string.Empty;
            try
            {
                IWebElement prospectEmploymentPTag = webElement.FindElement(By.CssSelector(".entity-result__primary-subtitle"));
                prospectEmploymentInfo = prospectEmploymentPTag.Text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not extract prospects employment information.");
            }

            return prospectEmploymentInfo;
        }
    }
}
