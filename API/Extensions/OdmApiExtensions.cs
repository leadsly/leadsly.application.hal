using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;

namespace API.Extensions
{
    public static class OdmApiExtensions
    {
        public static string GetAccessToken(this HttpContext context)
        {
            string token = string.Empty;

            string authorizationHeader = context.Request.Headers["Authorization"];

            if (authorizationHeader == null)
            {
                return string.Empty;
            }

            if(authorizationHeader.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                // use range operator https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges
                token = authorizationHeader[JwtBearerDefaults.AuthenticationScheme.Length..].Trim();
            }

            return token;
        }
    }
}
