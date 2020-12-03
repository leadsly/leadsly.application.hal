using API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;

namespace API.Filters
{

    public class GlobalControllerExceptionAttribute : ExceptionFilterAttribute
    {
        public GlobalControllerExceptionAttribute(ILogger<GlobalControllerExceptionAttribute> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<GlobalControllerExceptionAttribute> _logger;

        public override void OnException(ExceptionContext context)
        {
            // All user exceptions implement IWebApiException
            if (context.Exception is IWebApiException webApiException)
            {
                _logger.LogError(context.Exception, "Error occured processing request.");

                // Then return a problem detail
                ObjectResult result = new ObjectResult(new ProblemDetails
                {
                    Type = webApiException.Type,
                    Title = webApiException.Title ?? ReasonPhrases.GetReasonPhrase(webApiException.Status),
                    Status = webApiException.Status,
                    Detail = webApiException.Detail,
                    Instance = webApiException.Instance,
                })
                {
                    StatusCode = webApiException.Status
                };
                result.ContentTypes.Add(new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")));

                context.Result = result;
            }
            else if(context.Exception is OperationCanceledException)
            {
                _logger.LogInformation("Request was cancelled.");

                ObjectResult result = new ObjectResult(new ProblemDetails
                {
                    Title = "Request has been cancelled",
                    Status = StatusCodes.Status400BadRequest
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };

                context.Result = result;
            }

            _logger.LogError("Exception occured when processing request.");

            base.OnException(context);
        }
    }
}
