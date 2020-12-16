using API.Authentication;
using API.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.Middlewares
{
    public class JwtRefreshMiddleware
    {
        public JwtRefreshMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, ILogger<JwtRefreshMiddleware> logger)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        public async Task Invoke(HttpContext context)
        {   
            ClaimsPrincipal claimsPrincipal = context.Request.HttpContext.User;
            if ((claimsPrincipal == null || claimsPrincipal.Identity.IsAuthenticated == false))
            {
                string accessToken = context.GetAccessToken();                
                if (accessToken != string.Empty)
                {
                    _logger.LogDebug("Request is unauthenticated but contains jwt.");
                    UserManager<ApplicationUser> userManager = _httpContextAccessor.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
                    RoleManager<IdentityRole> roleManager = _httpContextAccessor.HttpContext.RequestServices.GetService<RoleManager<IdentityRole>>();
                    IAccessTokenService tokenService = _httpContextAccessor.HttpContext.RequestServices.GetService<IAccessTokenService>();

                    // request has token but it failed authentication. Attempt to renew the token
                    RenewAccessTokenResult result = await tokenService.TryRenewAccessToken(accessToken, userManager, roleManager);
                    bool succeeded = result.Succeeded;
                    _logger.LogDebug("Attempted to rewnew jwt. Result: {succeeded}", succeeded);

                    if (succeeded == true)
                    {
                        context.Request.Headers.Remove("Authorization");
                        context.Request.Headers.Add("Authorization", $"Bearer {result.AccessToken.access_token}");
                    }
                }                
            }

            await _next(context);
        }
    }
}
