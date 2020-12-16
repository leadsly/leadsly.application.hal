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

            string authorization = context.Request.Headers["Authorization"];

            if (authorization == null)
            {
                return string.Empty;
            }

            if(authorization.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring(JwtBearerDefaults.AuthenticationScheme.Length).Trim();
            }

            return token;
        }
    }
}
