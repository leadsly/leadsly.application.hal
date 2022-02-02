using Domain;
using Leadsly.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leadsly.Models.Database;

namespace Api.Controllers
{
    /// <summary>
    /// Base class the API controllers.
    /// </summary>
    public class ApiControllerBase : Controller
    {
    }
}
