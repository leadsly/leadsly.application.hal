﻿using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory
{
    public class CheckMessagesHistoryInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public string TargetMessage { get; set; }
        public string LeadslyUserFullName { get; set; }
        public string ProspectName { get; set; }
        public IWebElement MessageListItem { get; set; }
        public string CampaignProspectId { get; set; }
    }
}
