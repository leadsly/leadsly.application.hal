using API.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace API.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
    {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private static Task HandleException(HttpContext context, Exception ex)
        {
            // If the exception is not user based
            if (ex is not IOdmWebApiException)
            {
                // 500 if unexpected
                HttpStatusCode code = HttpStatusCode.InternalServerError; 

                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = (int)code;
                
            }

            return context.Response.WriteAsync("Status Code: 500; Internal Server Error.");
        }
    }
}
