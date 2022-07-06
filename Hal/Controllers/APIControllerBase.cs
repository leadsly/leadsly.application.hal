using Leadsly.Application.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hal.Controllers
{
    /// <summary>
    /// Base class the API controllers.
    /// </summary>
    public class ApiControllerBase : Controller
    {
        public string HalId { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// Produces api response when request fails to successfully complete.
        /// </summary>
        /// <param name="problemDetails"></param>
        /// <returns></returns>
        protected ObjectResult ProblemDetailsResult(ProblemDetails problemDetails)
        {
            problemDetails.Extensions.Add(ProblemDetailsExtensionKeys.Origin, HalId);

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes =
                {
                    new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")),
                }
            };
        }
        protected ObjectResult BadRequest_LeadslyAuthenticationError(List<Failure> errors)
        {
            Dictionary<string, string[]> errorsDictionary = errors.ToDictionary(x => Enum.GetName(x.Code ?? Codes.ERROR), x => new[] { x.Reason ?? "Error occured", x.Detail ?? "Operation failed to successfully complete" });

            return ProblemDetailsResult(new ValidationProblemDetails(errorsDictionary)
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.LeadslySocialAccountAuthenticationError,
                Instance = this.HttpContext.Request.Path.Value
            });
        }
        protected ObjectResult BadRequest(string details)
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = details,
                Instance = this.HttpContext.Request.Path.Value
            });
        }
    }
}
