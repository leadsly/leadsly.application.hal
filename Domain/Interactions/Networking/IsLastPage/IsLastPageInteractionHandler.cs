using Domain.Interactions.Networking.IsLastPage.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.IsLastPage
{
    public class IsLastPageInteractionHandler : IIsLastPageInteractionHandler
    {
        public IsLastPageInteractionHandler(
            ILogger<IsLastPageInteractionHandler> logger,
            ISearchPageFooterServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<IsLastPageInteractionHandler> _logger;
        private readonly ISearchPageFooterServicePOM _service;

        public bool HandleInteraction(InteractionBase interaction)
        {
            IsLastPageInteraction lastPageInteraction = interaction as IsLastPageInteraction;

            if (_service.IsLastPage(lastPageInteraction.WebDriver, lastPageInteraction.VerifyWithWebDriver, lastPageInteraction.CurrentPage, lastPageInteraction.TotalResults) == true)
            {
                return true;
            }

            return false;
        }
    }
}
