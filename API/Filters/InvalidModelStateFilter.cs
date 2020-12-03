using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;

namespace API.Filters
{
    class InvalidModelStateFilter : IActionFilter, IOrderedFilter
    {       
        public int Order { get; set; }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (context.ModelState.IsValid)
                return;

            ProblemDetails problemDetails = new ValidationProblemDetails(context.ModelState)
            {                
                Type = ProblemDetailsTypes.BadRequest,
                Status = 400,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.BadRequest,
                Instance = context.HttpContext.Request.Path.Value
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes =
                {
                    new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")),
                },
            };
        }
    }
}
