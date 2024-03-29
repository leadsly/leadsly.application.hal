﻿using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.GetTotalSearchResults;
using Domain.Interactions.Networking.GoToTheNextPage;
using Domain.Interactions.Networking.IsLastPage;
using Domain.Interactions.Networking.IsNextButtonDisabled;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.SearchResultsLimit;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.InstructionSets
{
    public class NetworkingInstructionSet : INetworkingInstructionSet
    {
        public NetworkingInstructionSet(
            ILogger<NetworkingInstructionSet> logger,
            INetworkingInteractionFacade interactionFacade)
        {
            _logger = logger;
            _interactionFacade = interactionFacade;
        }

        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects;

        private readonly ILogger<NetworkingInstructionSet> _logger;
        private readonly INetworkingInteractionFacade _interactionFacade;

        private int NumberOfToastErrorsThreshold = 2;
        private int NumberOfToastErrors { get; set; }
        private IList<ConnectionSentModel> ConnectionsSent { get; set; } = new List<ConnectionSentModel>();
        private IList<IWebElement> Prospects { get; set; } = new List<IWebElement>();
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; private set; } = new List<SearchUrlProgressModel>();
        public int NumberOfConnectionsSent { get; set; }
        public bool MonthlySearchLimitReached { get; private set; }
        public int TotalNumberOfSearchResults => _interactionFacade.TotalNumberOfSearchResults;

        #region Getters

        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls()
        {
            IList<SearchUrlProgressModel> searchUrlProgress = UpdatedSearchUrlsProgress;
            UpdatedSearchUrlsProgress = new List<SearchUrlProgressModel>();
            return searchUrlProgress;
        }

        public IList<ConnectionSentModel> GetConnectionsSent()
        {
            IList<ConnectionSentModel> connectionsSent = ConnectionsSent;
            ConnectionsSent = new List<ConnectionSentModel>();
            return connectionsSent;
        }

        public bool GetMonthlySearchLimitReached()
        {
            bool limitReached = MonthlySearchLimitReached;
            MonthlySearchLimitReached = false;
            return limitReached;
        }

        #endregion

        public void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults)
        {
            _logger.LogInformation("ConnectWithProspectsForSearchUrl executing");
            for (int currentPage = searchUrlProgress.LastPage; currentPage < totalResults + 1; currentPage++)
            {
                // after each iteration output ProspectList, to try and avoid race condition
                try
                {
                    UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);

                    if (GatherProspectsInteractions(webDriver, message) == false)
                    {
                        _logger.LogDebug("Gathering prospects on the search results page failed. Checking if next button is disabled");
                        if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                        {
                            _logger.LogDebug("Next button is disabled. Checking if this is the last page");
                            // if next button is disabled check if we are on the last page
                            if (IsLastPage(webDriver, currentPage, totalResults) == true)
                            {
                                _logger.LogDebug("This is the last page. Updating SearchUrlProgress request and breaking out of the loop.");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                break;
                            }
                            else
                            {
                                _logger.LogDebug("This is not the last page. Chcking if search result monthly limit has been reached.");
                                string currentUrl = webDriver.Url;
                                if (IsMonthlySearchLimitReached(webDriver) == true)
                                {
                                    _logger.LogDebug("Monthl search limit has been reached. Setting monthly search limit request and breaking.");
                                    UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                                    SetMonthlyLimitReachedRequest(true);
                                    break;
                                }
                                else
                                {
                                    // something simply went wrong we could retry or move on to the next page
                                    _logger.LogDebug("Monthyl search limit was not reached. Something appears to have gone wrong, updating SearchUrlProgress and breaking.");
                                    UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogDebug("Next button is not disabled. Trying to move onto the next page.");
                            if (GoToTheNextPage(webDriver) == false)
                            {
                                _logger.LogDebug("Failed to navigate to the next search results page. Updating SearchUrlProgress request and breaking.");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                break;
                            }
                        }

                        continue;
                    }

                    _logger.LogInformation("Creating request to save PrimaryProspect List");
                    OutputProspectList(message);

                    // if number of prospects comes back as zero continue to the next page
                    if (_interactionFacade.Prospects.Count == 0)
                    {
                        _logger.LogDebug("There aren't any prospects on this page that we can connect with. Checking if this is the last page.");
                        if (IsLastPage(webDriver, currentPage, totalResults))
                        {
                            _logger.LogDebug("This is the last page of this search url. Updating SearchUrlProgress and breaking");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, true);
                            break;
                        }
                        _logger.LogDebug("This is not the last page of this search url. Checking if next button is disabled");

                        if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                        {
                            _logger.LogDebug("The next button of the search url is disabled. Checking if we have reached the monthly search limit");
                            string currentUrl = webDriver.Url;
                            if (IsMonthlySearchLimitReached(webDriver) == true)
                            {
                                _logger.LogDebug("Monthly search limit has been reached. Setting MonthlySearchLimitReached request and breaking");
                                SetMonthlyLimitReachedRequest(true);
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                                break;
                            }

                            _logger.LogDebug("Next button is disabled and we have not reached monthly search limit. Something went wrong. Updating SearchUrlProgress request and breaking");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }

                        _logger.LogDebug("The next button of the search url is not disabled. Navigating to the next page.");
                        if (GoToTheNextPage(webDriver) == false)
                        {
                            _logger.LogDebug("Failed to navigate to the next search url page.");
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                            break;
                        }

                        continue;
                    }
                    else
                    {
                        Prospects = _interactionFacade.Prospects;
                    }

                    // if there are available prospects connect with them
                    ConnectWithProspectsOnCurrentPage(webDriver, message, currentPage, totalResults, searchUrlProgress.SearchUrlProgressId);

                    if (IsMaxNumberOfConnectionsSentReached(message.ProspectsToCrawl))
                    {
                        _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", NumberOfConnectionsSent, message.ProspectsToCrawl);
                        if (IsLastPage(webDriver, currentPage, totalResults))
                        {
                            // if number of available prospects on this page, matches the number of connection requests we have sent, we've exhausted this url
                            bool exhausted = Prospects.Count == ConnectionsSent.Count;
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, exhausted);
                        }
                        else
                        {
                            UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                        }

                        break;
                    }

                    // if this is the last page update search url progress request with exhausted property set to true                
                    if (IsLastPage(webDriver, currentPage, totalResults))
                    {
                        _logger.LogDebug("We are on the last page for this search result");
                        bool exhausted = Prospects.Count == ConnectionsSent.Count;
                        _logger.LogDebug("Has this search url been exhausted: {0}", exhausted);
                        UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url, exhausted);
                        break;
                    }
                    else
                    {
                        _logger.LogDebug("We are not on the last page. Checking if next button is disabled.");
                        if (CheckIfNextButtonIsNullOrDisabled(webDriver) == true)
                        {
                            _logger.LogDebug("Next button is disabled. Checking if we have reached our monthly search limit");
                            string currentUrl = webDriver.Url;
                            if (IsMonthlySearchLimitReached(webDriver) == true)
                            {
                                _logger.LogDebug("Monthly search limit has been reached. Setting MonthlyLimitReached request and updating SearchUrlProgress");
                                SetMonthlyLimitReachedRequest(true);
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, currentUrl);
                                break;
                            }
                            else
                            {
                                _logger.LogDebug("Monthly search limit has not been reached. Something went wrong and we can't continue to the next page. Updating SearchUrlProgress and breaking");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                break;
                            }
                        }
                        else
                        {
                            _logger.LogDebug("Next button is not disabled. Navigating to the next page.");
                            // if this is not the last page go to the next page
                            if (GoToTheNextPage(webDriver) == false)
                            {
                                _logger.LogDebug("Failed to navigate to the next page. Updating SearchUrlProgress and breaking.");
                                UpdateCurrentPage_SearchUrlProgress(searchUrlProgress.SearchUrlProgressId, currentPage, webDriver.Url);
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    OutputProspectList(message);
                }
            }
        }

        private void OutputProspectList(NetworkingMessageBody message)
        {
            List<PersistPrimaryProspectModel> persistPrimaryProspects = _interactionFacade.PersistPrimaryProspects;
            if (persistPrimaryProspects != null && persistPrimaryProspects.Count > 0)
            {
                _logger.LogInformation("Persisting {0} primary prospects for HalId {1}", persistPrimaryProspects.Count, message.HalId);
                PersistPrimaryProspects.Invoke(this, new PersistPrimaryProspectsEventArgs(message, persistPrimaryProspects));
            }
        }

        private void ConnectWithProspectsOnCurrentPage(IWebDriver webDriver, NetworkingMessageBody message, int currentPage, int totalResults, string searchUrlProgressId)
        {
            _logger.LogTrace("Executing ConnectWithProspectsOnCurrentPage");
            _logger.LogDebug("Number of connectable prospects: {0}", Prospects?.Count);
            ConnectWithProspectInteraction interaction = new()
            {
                CurrentPage = currentPage,
                TotalResults = totalResults,
                Message = message,
                WebDriver = webDriver
            };

            foreach (IWebElement prospect in Prospects)
            {
                if (IsMaxNumberOfConnectionsSentReached(message.ProspectsToCrawl))
                {
                    _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", NumberOfConnectionsSent, message.ProspectsToCrawl);
                    break;
                }

                interaction.Prospect = prospect;
                bool succeeded = _interactionFacade.HandleConnectWithProspectsInteraction(interaction);
                if (succeeded == true)
                {
                    NumberOfConnectionsSent += 1;
                    ConnectionsSent.Add(_interactionFacade.ConnectionSent);
                }
                else if (_interactionFacade.ErrorToastMessageDetected == true)
                {
                    NumberOfToastErrors += 1;
                }
                else
                {
                    _logger.LogDebug("The CampaignProspectRequest is null. Skipping this prospect and moving onto the next one");
                }

                if (NumberOfToastErrors == NumberOfToastErrorsThreshold)
                {
                    _logger.LogWarning("Number of error toast pop ups exceeded the threshold. Backing off from connecting with prospects");
                    // lets back off and treat this run as if we have successfully sent ALL connections. We will defer to another run or day
                    NumberOfConnectionsSent = message.ProspectsToCrawl;
                    // in case this wasn't the first interaction but subsequent, lets grab whatever we have in memory
                    ConnectionsSent.Add(_interactionFacade.ConnectionSent);
                }
            }
        }

        private void UpdateCurrentPage_SearchUrlProgress(string searchUrlProgressId, int currentPage, string currentUrl, bool exhausted = false)
        {
            SearchUrlProgressModel searchUrl = UpdatedSearchUrlsProgress.FirstOrDefault(r => r.SearchUrlProgressId == searchUrlProgressId);
            if (searchUrl != null)
            {
                searchUrl.SearchUrl = currentUrl;
                searchUrl.LastPage = currentPage;
                searchUrl.Exhausted = exhausted;
            }
        }

        private bool GatherProspectsInteractions(IWebDriver webDriver, NetworkingMessageBody message)
        {
            GatherProspectsInteraction gatherProspectsInteraction = new()
            {
                WebDriver = webDriver,
                Message = message,
            };

            return _interactionFacade.HandleGatherProspectsInteraction(gatherProspectsInteraction);
        }

        private bool CheckIfNextButtonIsNullOrDisabled(IWebDriver webDriver)
        {
            InteractionBase interaction = new IsNextButtonDisabledInteraction
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleCheckIfNextButtonIsNullOrDisabledInteraction(interaction);
        }

        private bool IsLastPage(IWebDriver webDriver, int currentPage, int totalResults)
        {
            InteractionBase interaction = new IsLastPageInteraction
            {
                WebDriver = webDriver,
                CurrentPage = currentPage,
                TotalResults = totalResults,
                VerifyWithWebDriver = false
            };

            return _interactionFacade.HandleIsLastPageInteraction(interaction);
        }

        private bool IsMonthlySearchLimitReached(IWebDriver webDriver)
        {
            SearchResultsLimitInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleSearchResultsLimitInteraction(interaction);
        }

        private void SetMonthlyLimitReachedRequest(bool limitReached)
        {
            MonthlySearchLimitReached = limitReached;
        }

        private bool GoToTheNextPage(IWebDriver webDriver)
        {
            InteractionBase interaction = new GoToTheNextPageInteraction()
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleGoToTheNextPageInteraction(interaction);
        }

        private bool IsMaxNumberOfConnectionsSentReached(int numberOfProspectsToCrawl)
        {
            if (NumberOfConnectionsSent >= numberOfProspectsToCrawl)
            {
                _logger.LogDebug("Number of connections sent has reached the max number of connections that can be sent");
                return true;
            }
            _logger.LogDebug("Number of connections sent has not reached the max number of connections that can be sent");

            return false;
        }

        public bool GetTotalnumberOfSearchResultsInteraction(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress)
        {
            InteractionBase interaction = new GetTotalSearchResultsInteraction
            {
                WebDriver = webDriver,
                TotalNumberOfResults = searchUrlProgress.TotalSearchResults
            };

            return _interactionFacade.HandleGetTotalNumberOfSearchResults(interaction);
        }

        public bool NoSearchResultsDisplayedInteraction(IWebDriver webDriver)
        {
            NoResultsFoundInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleNoResultsFoundInteraction(interaction);
        }

        public void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle)
        {
            if (UpdatedSearchUrlsProgress.Any(req => req.SearchUrlProgressId == searchUrlProgressId) == false)
            {
                UpdatedSearchUrlsProgress.Add(new()
                {
                    SearchUrlProgressId = searchUrlProgressId,
                    StartedCrawling = true,
                    LastPage = currentPage,
                    SearchUrl = currentUrl,
                    TotalSearchResults = totalResults,
                    WindowHandleId = currentWindowHandle
                });
            }
        }
    }
}
