using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

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
        /// Bad request when there is an issue signing up a new user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult BadRequest_UserNotCreated(IEnumerable<IdentityError> errors)
        {
            Dictionary<string, string[]> errorsDictionary = errors.ToDictionary(x => x.Code, x => new[] { x.Description });

            return ProblemDetailsResult(new ValidationProblemDetails(errorsDictionary)
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.RegistrationDetail,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }

        /// <summary>
        /// Bad request when user is not found.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_UserRegistrationError()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.RegistrationErrorDetail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when cannot find user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        protected ObjectResult Unauthorized_InvalidCredentials()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedDetail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }
    }
}
