using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ProspectListProvider : IProspectListProvider
    {
        public ProspectListProvider(
            ILinkedInHomePage linkedInHomePage,
            ILinkedInMyNetworkPage linkedInMyNetworkPage,
            ILogger<ProspectListProvider> logger,
            IWebDriverProvider webDriverProvider,
            IHalIdentity halIdentity,
            ILinkedInNavBar linkedInNavBar,
            ICampaignPhaseProcessingService campaignProcessingPhase,
            ILinkedInHtmlParser linkedInHtmlParser,
            ILinkedInSearchPage linkedInSearchPage,
            IHalOperationConfigurationProvider halConfigurationProvider)
        {
            _logger = logger;
            _linkedInMyNetworkPage = linkedInMyNetworkPage;
            _campaignProcessingPhase = campaignProcessingPhase;
            _linkedInSearchPage = linkedInSearchPage;
            _webDriverProvider = webDriverProvider;
            _linkedInNavBar = linkedInNavBar;
            _linkedInHomePage = linkedInHomePage;
            _linkedInHtmlParser = linkedInHtmlParser;
            _halConfigurationProvider = halConfigurationProvider;
            _halIdentity = halIdentity;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInNavBar _linkedInNavBar;
        private readonly ILogger<ProspectListProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInHtmlParser _linkedInHtmlParser;
        private readonly ILinkedInMyNetworkPage _linkedInMyNetworkPage;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHalOperationConfigurationProvider _halConfigurationProvider;
        private readonly IHalIdentity _halIdentity;

        public async Task<HalOperationResult<T>> ExecutePhase<T>(ProspectListBody message) 
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.ProspectList,
                ChromeProfileName = message.ChromeProfile,
                PageUrls = message.SearchUrls
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            return await ProspectList<T>(webDriver, message);
        }


        private List<PrimaryProspect> CollectProspects(IWebDriver webDriver, string searchUrl, int totalResults)            
        {
            string queryParam = "&page={num}";
            string nextPageUrl = searchUrl;
            List<IWebElement> prospectsAsWebElements = new();
            List<PrimaryProspect> prospects = new();
            for (int i = 1; i < totalResults; i++)
            {
                int pageNum = i;
                nextPageUrl += queryParam.Replace("{num}", pageNum.ToString());
                HalOperationResult<INewTabOperation> newTabOperation = _webDriverProvider.NewTab<INewTabOperation>(webDriver);
                if(newTabOperation.Succeeded == false)
                {
                    return null;
                }

                var a = GoToPage<IOperationResponse>(webDriver, nextPageUrl);

                HalOperationResult<IGatherProspects> result = _linkedInSearchPage.GatherProspects<IGatherProspects>(webDriver);
                if(result.Succeeded == false) 
                {
                    //continue
                    continue;
                }

                List<IWebElement> propsAsWebElements = result.Value.ProspectElements;
                prospectsAsWebElements.AddRange(propsAsWebElements);

                if (i == 1)
                    break;
            }

            prospects = CreatePrimaryProspects(prospectsAsWebElements);
            // close all opened tabs


            return prospects;
        }

        private List<PrimaryProspect> CreatePrimaryProspects(List<IWebElement> prospects)
        {
            List<PrimaryProspect> primaryProspects = new();

            foreach (IWebElement webElement in prospects)
            {
                primaryProspects.Add(new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = GetProspectsName(webElement),
                    ProfileUrl = GetProspectsProfileUrl(webElement),
                    SearchResultAvatarUrl = GetProspectsSearchResultAvatarUrl(webElement),
                    Area = GetProspectsArea(webElement),
                    EmploymentInfo = GetProspectsEmploymentInfo(webElement)
                });
            }

            return primaryProspects;
        }

        private string GetProspectsName(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            return innerText[0] ?? string.Empty;
        }

        private string GetProspectsProfileUrl(IWebElement webElement)
        {
            return "";
        }
        private string GetProspectsSearchResultAvatarUrl(IWebElement webElement)
        {
            IWebElement img = webElement.FindElement(By.CssSelector(".presence-entity")).FindElement(By.TagName("img"));
            return img.GetAttribute("src") ?? string.Empty;
        }

        private string GetProspectsArea(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            return innerText[2] ?? string.Empty;
        }

        private string GetProspectsEmploymentInfo(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            return innerText[1] ?? string.Empty;
        }

        private async Task<HalOperationResult<T>> ProspectList<T>(IWebDriver webDriver, ProspectListBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            List<PrimaryProspect> prospects = new();
            foreach (string searchUrl in message.SearchUrls)
            {
                result = GoToPage<T>(webDriver, searchUrl);
                if(result.Succeeded == false)
                {
                    return result;
                }

                result = _linkedInSearchPage.GetTotalSearchResults<T>(webDriver);
                if(result.Succeeded == false)
                {
                    return result;
                }

                int totalResults = ((IGetTotalNumberOfResults)result.Value).NumberOfResults;

                List<PrimaryProspect> primaryProspects = CollectProspects(webDriver, searchUrl, totalResults);

                if(primaryProspects != null)
                {
                    prospects.AddRange(primaryProspects);
                }
            }

            // result.Value = prospects;
            return result;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                // first navigate to messages
                result = _linkedInHomePage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }
    }
}
