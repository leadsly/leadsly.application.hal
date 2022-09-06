using Domain.Interactions.Networking.ConnectWithProspect.Interfaces;
using Domain.Models.SendConnections;
using Domain.POMs.Dialogs;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace Domain.Interactions.Networking.ConnectWithProspect
{
    public class ConnectWithProspectInteractionHandler : IConnectWithProspectInteractionHandler
    {
        public ConnectWithProspectInteractionHandler(
            ILogger<ConnectWithProspectInteractionHandler> logger,
            IHumanBehaviorService humanBehaviorService,
            ISearchPageDialogManager searchPageDialogManager,
            ILinkedInSearchPage linkedInSearchPage)
        {
            _linkedInSearchPage = linkedInSearchPage;
            _searchPageDialogManager = searchPageDialogManager;
            _humanBehaviorService = humanBehaviorService;
            _logger = logger;
        }

        public ConnectionSentModel ConnectionSent { get; private set; }

        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ISearchPageDialogManager _searchPageDialogManager;
        private readonly ILogger<ConnectWithProspectInteractionHandler> _logger;

        public bool HandleInteraction(InteractionBase interaction)
        {
            ConnectWithProspectInteraction connectWithProspectInteraction = interaction as ConnectWithProspectInteraction;
            _logger.LogTrace("Preparing to connect with prospect.");

            // send connection
            bool sendConnectionSuccess = SendConnection(connectWithProspectInteraction.WebDriver, connectWithProspectInteraction.Prospect);
            if (sendConnectionSuccess == false)
            {
                _logger.LogDebug("Sending connection to the given prospect failed.");
                // if there was a failure attempt to close modal dialog if it is open
                _humanBehaviorService.RandomWaitMilliSeconds(850, 2000);
                _searchPageDialogManager.TryCloseModal(connectWithProspectInteraction.WebDriver);

                return false;
            }

            ConnectionSent = CreateCampaignProspects(connectWithProspectInteraction.Prospect);

            return true;
        }

        private bool SendConnection(IWebDriver webDriver, IWebElement prospect)
        {
            bool sendConnectionSuccess = true;
            _logger.LogInformation("[SendConnectionRequests]: Sending connection request to the given prospect");

            _humanBehaviorService.RandomWaitMilliSeconds(700, 3000);
            HalOperationResult<IOperationResponse> sendConnectionResult = _linkedInSearchPage.SendConnectionRequest<IOperationResponse>(prospect);
            if (sendConnectionResult.Succeeded == false)
            {
                _logger.LogDebug("Clicking 'Connect' button on the prospect failed");
                sendConnectionSuccess = false;
            }
            else
            {
                _humanBehaviorService.RandomWaitMilliSeconds(700, 1500);
                sendConnectionSuccess = _searchPageDialogManager.HandleConnectWithProspectModal(webDriver);
            }

            return sendConnectionSuccess;
        }

        private ConnectionSentModel CreateCampaignProspects(IWebElement prospect)
        {
            return new ConnectionSentModel()
            {
                ConnectionSentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Name = GetProspectsName(prospect),
                ProfileUrl = GetProspectsProfileUrl(prospect)
            };
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
    }
}
