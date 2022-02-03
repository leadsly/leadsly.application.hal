using Domain;
using Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    /// <summary>
    /// Healthcheck controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]    
    public class HealthCheckController : ApiControllerBase
    {
        public HealthCheckController(ILogger<HealthCheckController> logger)
        {
            _logger = logger;            
        }

        private readonly ILogger<HealthCheckController> _logger;

        /// <summary>
        /// Gets applications name and version. If returned indicates api is successfully running.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/healthcheck")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            _logger.LogTrace("Healthcheck action executed.");           
            return new JsonResult(new
            {
                APIVersion = typeof(Startup).Assembly.GetName().Version.ToString()
            });
        }
    }
}
