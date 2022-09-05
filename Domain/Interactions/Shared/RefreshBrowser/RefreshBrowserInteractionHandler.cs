using Domain.Interactions.Shared.RefreshBrowser.Interfaces;
using Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Shared.RefreshBrowser
{
    public class RefreshBrowserInteractionHandler : IRefreshBrowserInteractionHandler
    {
        public RefreshBrowserInteractionHandler(ILogger<RefreshBrowserInteractionHandler> logger, IWebDriverProvider webDriverProvider)
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        private readonly ILogger<RefreshBrowserInteractionHandler> _logger;
        private readonly IWebDriverProvider _webDriverProvider;

        public bool HandleInteraction(InteractionBase interaction)
        {
            RefreshBrowserInteraction refreshInteraction = interaction as RefreshBrowserInteraction;
            bool succeeded = _webDriverProvider.Refresh(refreshInteraction.WebDriver);
            if (succeeded == false)
            {
                // handle failures or retires here
            }

            return succeeded;
        }
    }
}
