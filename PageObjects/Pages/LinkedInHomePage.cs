using Domain.POMs.Pages;
using Microsoft.Extensions.Logging;

namespace PageObjects.Pages
{
    public class LinkedInHomePage : LeadslyWebDriverBase, ILinkedInHomePage
    {
        public LinkedInHomePage(ILogger<LinkedInHomePage> logger) : base(logger)
        {            
            this._logger = logger;
            
        }
        private readonly ILogger<LinkedInHomePage> _logger;        

    }
}
