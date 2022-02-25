using Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Domain.Models;

namespace Hal.Controllers
{
    /// <summary>
    /// Web driver controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WebDriverController : ApiControllerBase
    {
        public WebDriverController(ISupervisor supervisor)
        {
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create([FromBody] InstantiateWebDriver newWebDriverRequest)
        {
            WebDriverInformation webDriverInformation = _supervisor.CreateWebDriver(newWebDriverRequest);

            return Created("/webdriver", webDriverInformation);
        }
    }
}
