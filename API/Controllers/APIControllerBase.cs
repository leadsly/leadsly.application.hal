using API.Filters;
using Domain.Models;
using Domain.Supervisor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace API.Controllers
{
    public class APIControllerBase : Controller
    {
        protected ObjectResult ProblemDetailsResult(ProblemDetails problemDetails)
        {
            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes =
                {
                    new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")),
                }
            };
        }

        /// <summary>
        /// Use as an example to show how to return user handled errors.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult NotFound_ItemIdIsInvalidOrNoRights(string id)
        {
            return ProblemDetailsResult(new ProblemDetails()
            {
                Type = ProblemDetailsTypes.NotFound,
                Status = StatusCodes.Status404NotFound,
                Title = ReasonPhrases.GetReasonPhrase(404),
                Detail = $"The item '{id}' does not exist or you do not have rights to it",
                Instance = this.HttpContext.Request.Path.Value,
            });
        }
    }
}
