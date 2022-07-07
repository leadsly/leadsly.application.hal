using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Hal.Filters
{
    public class AuthAttemptCountAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.HttpContext.Request.Headers.ContainsKey("X-Auth-Attempt-Count") == false)
            {
                context.HttpContext.Response.Headers.Add("X-Auth-Attempt-Count", "1");
            }
            else
            {
                context.HttpContext.Request.Headers.TryGetValue("X-Auth-Attempt-Count", out StringValues attemptCount);
                attemptCount = (int.Parse(attemptCount) + 1).ToString();
                context.HttpContext.Response.Headers.Add("X-Auth-Attempt-Count", attemptCount);
            }

            base.OnResultExecuting(context);
        }
    }
}
