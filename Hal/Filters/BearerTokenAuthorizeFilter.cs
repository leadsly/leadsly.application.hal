using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;

namespace Hal.Filters
{
    public class BearerTokenAuthorizeFilter : AuthorizeFilter
    {
        public BearerTokenAuthorizeFilter(AuthorizationPolicy policy)
            : base(policy) { }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);

            if (context.Result is ChallengeResult)
            {
                // Then return a problem detail
                ObjectResult result = new ObjectResult(new ProblemDetails
                {
                    Type = ProblemDetailsTypes.Unauthorized,
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = ProblemDetailsDescriptions.Unauthorized
                });

                result.ContentTypes.Add(new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")));

                context.Result = result;
                await context.HttpContext.ChallengeAsync();
            }
            else if (context.Result is ForbidResult)
            {
                // Then return a problem detail
                ObjectResult result = new ObjectResult(new ProblemDetails
                {
                    Type = ProblemDetailsTypes.Forbidden,
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden),
                    Status = StatusCodes.Status403Forbidden,
                    Detail = ProblemDetailsDescriptions.ForbiddenDetail
                });

                result.ContentTypes.Add(new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")));

                context.Result = result;
                await context.HttpContext.ForbidAsync();
            }

        }
    }
}
