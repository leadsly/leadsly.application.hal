using Domain.Interactions.Networking.GatherProspects.Interfaces;
using Domain.Models.ProspectList;
using Domain.Models.RabbitMQMessages;
using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Interactions.Networking.GatherProspects
{
    public class GatherProspectsInteractionHandler : IGatherProspectsInteractionHandler
    {
        public GatherProspectsInteractionHandler(ILogger<GatherProspectsInteractionHandler> logger, IHumanBehaviorService humanBehaviorService, ILinkedInSearchPage linkedInSearchPage)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _linkedInSearchPage = linkedInSearchPage;
        }

        public List<PersistPrimaryProspectModel> PersistPrimaryProspects
        {
            get
            {
                // anytime we get values from this property, reset it back to zero
                List<PersistPrimaryProspectModel> requests = _persistPrimaryProspectRequests;
                _persistPrimaryProspectRequests = new List<PersistPrimaryProspectModel>();
                return requests;
            }
            set
            {
                _persistPrimaryProspectRequests = value;
            }

        }

        private List<PersistPrimaryProspectModel> _persistPrimaryProspectRequests = new List<PersistPrimaryProspectModel>();
        public IList<IWebElement> Prospects { get; set; }
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly ILogger<GatherProspectsInteractionHandler> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private Func<IWebElement, bool> ConnectableProspects
        {
            get
            {
                return (r) =>
                {
                    IWebElement actionBtn = _linkedInSearchPage.GetProspectsActionButton(r);
                    if (actionBtn == null)
                    {
                        return false;
                    }
                    return actionBtn.Text == ApiConstants.PageObjectConstants.Connect;
                };
            }
        }

        public bool HandleInteraction(InteractionBase interaction)
        {
            GatherProspectsInteraction gatherProspectsInteraction = interaction as GatherProspectsInteraction;
            NetworkingMessageBody message = gatherProspectsInteraction.Message as NetworkingMessageBody;
            IList<IWebElement> connectableProspects = GetConnectableProspects(gatherProspectsInteraction.WebDriver);
            if (connectableProspects == null)
            {
                return false;
            }
            else
            {
                Prospects = connectableProspects;
                ProspectList(gatherProspectsInteraction.WebDriver);
            }

            return true;
        }

        private IList<IWebElement> GetConnectableProspects(IWebDriver webDriver)
        {
            _logger.LogTrace("Crawling all of prospects on this page");
            // filter down the list to only those prospects that we can connect with
            IList<IWebElement> connectableProspectsOnThisPage = CrawlProspects(webDriver)?.Where(ConnectableProspects)?.ToList();

            webDriver.ScrollTop(_humanBehaviorService);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 3000);

            IWebElement areResultsHelpfulText = _linkedInSearchPage.AreResultsHelpfulPTag(webDriver);
            _humanBehaviorService.RandomClickElement(areResultsHelpfulText);

            return connectableProspectsOnThisPage;
        }

        private IList<IWebElement> CrawlProspects(IWebDriver webDriver)
        {
            bool? didSearchResultsDisplay = DidSearchResultsDisplay(webDriver);
            if (didSearchResultsDisplay == false || didSearchResultsDisplay == null)
            {
                return null;
            }

            IList<IWebElement> rawProspects = _linkedInSearchPage.GatherProspects(webDriver);
            if (rawProspects == null)
            {
                return null;
            }

            _logger.LogInformation("Number of raw prospects discovered is {0}", rawProspects);

            // we need to perform a random scroll
            if (rawProspects.Count > 3)
            {
                _logger.LogDebug("Performing random scroll");
                _linkedInSearchPage.ScrollIntoView(rawProspects[2], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            if (rawProspects.Count > 8)
            {
                _linkedInSearchPage.ScrollIntoView(rawProspects[7], webDriver);
                _humanBehaviorService.RandomWaitMilliSeconds(3000, 5000);
            }

            return rawProspects;
        }

        private bool? DidSearchResultsDisplay(IWebDriver webDriver)
        {
            bool isNoSearchResultsContainerDisplayed = _linkedInSearchPage.IsNoSearchResultsContainerDisplayed(webDriver);
            if (isNoSearchResultsContainerDisplayed == true)
            {
                _humanBehaviorService.RandomWaitSeconds(2, 5);
                bool? retrySearchOperationResult = _linkedInSearchPage.ClickRetrySearch(webDriver, 2, 5);
                if (retrySearchOperationResult == null)
                {
                    _logger.LogDebug("Failed to locate 'Search Result' retry button");
                    return null;
                }
                else if (retrySearchOperationResult == false)
                {
                    _logger.LogWarning("Search results container was not located.");
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        private void ProspectList(IWebDriver webDriver)
        {
            _logger.LogDebug($"Persisting {Prospects.Count} prospects as part of the ProspectListPhase.");
            List<PersistPrimaryProspectModel> collectedProspects = CreatePrimaryProspects(Prospects);
            if (collectedProspects.Count > 0)
            {
                PersistPrimaryProspects = collectedProspects;
            }
        }

        private List<PersistPrimaryProspectModel> CreatePrimaryProspects(IList<IWebElement> prospects)
        {
            List<PersistPrimaryProspectModel> primaryProspects = new List<PersistPrimaryProspectModel>();
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
