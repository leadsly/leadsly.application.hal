﻿using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class HealthCheckController : ApiControllerBase
    {
        public HealthCheckController(ILogger<HealthCheckController> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<HealthCheckController> _logger;

        [HttpGet("/healthcheck")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            _logger.LogDebug("Healthcheck action executed.");           
            return new JsonResult(new HealthCheckViewModel 
            {
                APIVersion = typeof(Startup).Assembly.GetName().Version.ToString()
            });
        }
    }
}
