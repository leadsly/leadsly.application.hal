using Leadsly.Application.Model;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Hal.Filters
{
    public class CustomHeaderFilter : IActionFilter
    {
        private readonly IHalIdentity _halIdentity;
        private readonly ILogger<CustomHeaderFilter> _logger;
        public CustomHeaderFilter(IHalIdentity halIdentity, ILogger<CustomHeaderFilter> logger)
        {
            _halIdentity = halIdentity;
            _logger = logger;
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if(_halIdentity.Id == null)
            {
                _logger.LogWarning("Hal does not have unique identification configured");
            }

            context.HttpContext.Response.Headers.Add(CustomHeaderKeys.Origin, _halIdentity.Id);
        }

    }
}
