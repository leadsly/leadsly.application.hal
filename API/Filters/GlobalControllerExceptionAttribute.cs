using API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.Filters
{

    public class GlobalControllerExceptionAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            // All user exceptions implement IWebApiException
            if (context.Exception is IWebAPiException webApiException)
            {
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

            base.OnException(context);
        }
    }
}
