using System.Collections.Generic;

namespace Leadsly.Application.Model
{
    public class WebDriverOperationData
    {
        public BrowserPurpose BrowserPurpose { get; set; }
        public string RequestedWindowHandleId { get; set; }
        public string ChromeProfileName { get; set; }
        public string PageUrl { get; set; }
        public List<string> PageUrls { get; set; }
    }
}
