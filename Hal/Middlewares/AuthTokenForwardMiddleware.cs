using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Hal.Middlewares
{
    public class AuthTokenForwardMiddleware
    {
        private readonly RequestDelegate next;

        public AuthTokenForwardMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string access_token = await context.GetTokenAsync("access_token") ?? context.Request.Headers["Authorization"];

            await next(context);

            // add access_token to the response
            if (access_token != null)
            {
                context.Response.Headers.Add("Authorization", access_token);
            }
        }
    }
}
