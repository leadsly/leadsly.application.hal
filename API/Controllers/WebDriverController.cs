using Leadsly.Models.Database;
using Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Leadsly.Models;

namespace API.Controllers
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
        public IActionResult Create([FromBody] CreateWebDriver newWebDriverRequest)
        {
            WebDriverInformation webDriverInformation = _supervisor.CreateWebDriver(newWebDriverRequest);

            return Created("/webdriver", null);
        }

        [HttpDelete]
        [AllowAnonymous]
        public IActionResult Delete([FromBody] DestroyWebDriver destroyWebDriverRequest)
        {
            bool webDriverDestroyed = _supervisor.DestroyWebDriver(destroyWebDriverRequest);

            if (webDriverDestroyed == false)
            {
                return BadRequest_FailedToDestroyWebDriver();
            }

            return Ok();
        }
    }
}
