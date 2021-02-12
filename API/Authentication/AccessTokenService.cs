using API.Authentication.Jwt;
using Domain.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace API.Authentication
{
    public class AccessTokenService : IAccessTokenService
    {
        public AccessTokenService(IConfiguration configuration, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions, IClaimsIdentityService claimsIdentityService)
        {
            _configuration = configuration;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _claimsIdentityService = claimsIdentityService;
        }

        private readonly IConfiguration _configuration;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IClaimsIdentityService _claimsIdentityService;

        private IJsonSerializer Serializer => new JsonNetSerializer();
        private IDateTimeProvider Provider => new UtcDateTimeProvider();
        private IBase64UrlEncoder UrlEncoder => new JwtBase64UrlEncoder();
        private IJwtAlgorithm Algorithm => new HMACSHA256Algorithm();

        public async Task<ApplicationAccessToken> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity)
        {
            return new ApplicationAccessToken
            {
                access_token = await _jwtFactory.GenerateEncodedJwtAsync(userId, identity),
                expires_in = (long)_jwtOptions.ValidFor.TotalSeconds
            };
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken)
        {
            IConfigurationSection jwtAppSettingOptions = _configuration.GetSection(nameof(JwtIssuerOptions));

            TokenValidationParameters expiredTokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration[ApiConstants.VaultKeys.JwtSecret])),
                ValidateLifetime = false,

                RequireSignedTokens = true,
                RequireExpirationTime = true,
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = null;

            ClaimsPrincipal principal = tokenHandler.ValidateToken(expiredAccessToken, expiredTokenValidationParameters, out securityToken);
            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new OdmSecurityTokenException();
            }

            return principal;
        }

        public async Task<RenewAccessTokenResult> TryRenewAccessToken(string expiredAccessToken, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            RenewAccessTokenResult result = new RenewAccessTokenResult();

            ClaimsPrincipal claimsPrincipal = GetPrincipalFromExpiredToken(expiredAccessToken);

            Claim userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return result;
            }

            ApplicationUser appUser = await userManager.FindByIdAsync(userId.Value);

            if (appUser == null)
            {
                return result;
            }

            string refreshToken = await userManager.GetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.RememberMe);

            bool isValid = await userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.Purpose, refreshToken);

            if (isValid == false)
            {
                return result;
            }

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser, userManager, roleManager);

            await userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.RememberMe);

            string newRefreshToken = await userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.Purpose);

            IdentityResult settingNewTokenResult = await userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.RememberMe, newRefreshToken);

            if(settingNewTokenResult.Succeeded == false)
            {
                return result;
            }

            ApplicationAccessToken accessToken = await GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            result.Succeeded = true;

            result.AccessToken = accessToken;

            return result;
        }
    }    
}
