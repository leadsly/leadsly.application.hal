using OpenQA.Selenium;

namespace Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince
{
    public class GetAllRecentlyAddedSinceInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public int NumOfHoursAgo { get; set; }
        public string TimezoneId { get; set; }
    }
}
