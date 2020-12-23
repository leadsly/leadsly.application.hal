using API.Authentication;
using API.Extensions;
using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using System.Text.Encodings.Web;
using API.Services;
using MimeKit;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class AuthController : ApiControllerBase
    {
        public AuthController(            
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,            
            RoleManager<IdentityRole> roleManager,
            UrlEncoder urlEncoder,
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            ILogger<AuthController> logger)
        {            
            _tokenService = tokenService;
            _claimsIdentityService = claimsIdentityService;            
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _urlEncoder = urlEncoder;
            _emailService = emailService;
            _templateGenerator = templateGenerator;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _logger = logger;
        }
        
        private readonly IAccessTokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly UrlEncoder _urlEncoder;
        private readonly IClaimsIdentityService _claimsIdentityService;        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<AuthController> _logger;

        [HttpPost]
        [AllowAnonymous]
        [Route("signin")]
        public async Task<IActionResult> SignIn([FromBody] SigninUserModel signin, CancellationToken ct = default)
        {
            _logger.LogDebug("SignIn action executed.");

            if (string.IsNullOrEmpty(signin.Password))
            {
                return Unauthorized_InvalidCredentials();
            }

            ApplicationUser appUser = await _userManager.FindByEmailAsync(signin.Email);

            if (appUser == null)
            {
                return Unauthorized_InvalidCredentials();
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
            
            if (await _userManager.CheckPasswordAsync(appUser, signin.Password) == false)
            {
                await _userManager.AccessFailedAsync(appUser);

                if (await _userManager.IsLockedOutAsync(appUser))
                {
                    return Unauthorized_AccountLockedOut();
                }

                int failedAttempts = await _userManager.GetAccessFailedCountAsync(appUser);

                return Unauthorized_InvalidCredentials(failedAttempts);
            }

            // if user successfully logged in reset access failed count back to zero.
            await _userManager.ResetAccessFailedCountAsync(appUser);

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser, _userManager, _roleManager);

            ApplicationAccessToken accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.RememberMe);

            if(signin.RememberMe == true)
            {
                string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.Purpose);

                await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RefreshTokenProvider.Name, ApiConstants.DataTokenProviders.RefreshTokenProvider.RememberMe, refreshToken);                
            }            

            return Ok(accessToken);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignupUserModel signupModel, CancellationToken ct = default)
        {
            _logger.LogDebug("Signup action executed.");

            if(string.Equals(signupModel.Password, signupModel.ConfirmPassword, StringComparison.OrdinalIgnoreCase) == false)
            {
                _logger.LogError("Confirm password and password do not match.");

                return BadRequest_UserRegistrationError();
            }            

            ApplicationUser newUser = new ApplicationUser
            {
                Email = signupModel.Email,
                UserName = signupModel.Email
            };

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            IdentityResult result = await _userManager.CreateAsync(newUser, signupModel.Password);

            if (result.Succeeded == false)
            {
                _logger.LogError("User registration data is invalid or missing.");

                return BadRequest_UserNotCreated(result.Errors);
            }

            ApplicationUser signedupUser = await _userManager.FindByEmailAsync(signupModel.Email);

            if (signedupUser == null)
            {
                _logger.LogError($"Unable to find user by email: { signupModel.Email }. Attempted to retrieve newly registered user to generate access token.");

                return BadRequest_UserRegistrationError();
            }

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupUser, _userManager, _roleManager);

            ApplicationAccessToken accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupUser.Id, claimsIdentity);

            return Ok(accessToken);
        }        

        [HttpPost]
        [AllowAnonymous]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            _logger.LogDebug("RefreshToken action executed.");

            RenewAccessTokenResult result = new RenewAccessTokenResult();

            string expiredAccessToken = HttpContext.GetAccessToken();

            if(expiredAccessToken != string.Empty)
            {
                // request has token but it failed authentication. Attempt to renew the token
                result = await _tokenService.TryRenewAccessToken(expiredAccessToken, _userManager, _roleManager);
                bool succeeded = result.Succeeded;
                _logger.LogDebug("Attempted to rewnew jwt. Result: {succeeded}", succeeded);
            }

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("external-signin-google")]
        public async Task<IActionResult> ExternalSigninGoogle([FromBody] SocialUserModel externalUser)
        {
            _logger.LogDebug("ExternalSigninGoogle action executed.");
            ApplicationUser appUser = await _userManager.FindByEmailAsync(externalUser.Email);

            ApplicationAccessToken accessToken;

            // Sign user in
            if (appUser != null)
            {
                bool result = await _userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Google, ApiConstants.DataTokenProviders.ExternalLoginProviders.IdToken, externalUser.IdToken);

                if (result == true)
                {
                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser, _userManager, _roleManager);

                    accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);
                }
                else
                {
                    _logger.LogError("External provider token was invalid.");

                    return Unauthorized_InvalidExternalProviderToken();
                }

            }
            // Sign user up
            else
            {
                appUser = new ApplicationUser
                {
                    Email = externalUser.Email,
                    FirstName = externalUser.FirstName,
                    LastName = externalUser.LastName,
                    UserName = externalUser.Email,
                    ExternalProvider = externalUser.Provider,
                    PhotoUrl = externalUser.PhotoUrl,
                    ExternalProviderUserId = externalUser.Id
                };

                PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
                string password = passwordHasher.HashPassword(appUser, Guid.NewGuid().ToString());

                IdentityResult result = await _userManager.CreateAsync(appUser, password);

                if (result.Succeeded == false)
                {
                    _logger.LogError("User registration data is invalid or missing.");

                    return BadRequest_UserNotCreated(result.Errors);
                }

                ApplicationUser signedupExternalUser = await _userManager.FindByEmailAsync(appUser.Email);

                if (signedupExternalUser == null)
                {
                    _logger.LogError($"Unable to find user by email: { signedupExternalUser.Email }. Attempted to retrieve newly registered user to generate access token.");

                    return BadRequest_UserRegistrationError();
                }

                ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupExternalUser, _userManager, _roleManager);

                accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupExternalUser.Id, claimsIdentity);
            }

            return Ok(accessToken);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("external-signin-facebook")]
        public async Task<IActionResult> ExternalSigninFacebook([FromBody] SocialUserModel externalUser)
        {
            _logger.LogDebug("ExternalSigninFacebook action executed.");

            ApplicationAccessToken accessToken;

            ApplicationUser appUser = await _userManager.FindByEmailAsync(externalUser.Email);

            // Sign user in
            if (appUser != null)
            {
                _logger.LogInformation("User exists.");

                bool result = await _userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook, ApiConstants.DataTokenProviders.ExternalLoginProviders.AuthToken, externalUser.AuthToken);

                if (result == true)
                {
                    _logger.LogInformation("External provider token was successfully validated.");

                    IdentityResult removeAuthTokenResult = await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook, ApiConstants.DataTokenProviders.ExternalLoginProviders.AuthToken);
                    bool removalSucceeded = removeAuthTokenResult.Succeeded;

                    _logger.LogDebug("Was Facebook auth token successfully removed: {removalSucceeded}.", removalSucceeded);

                    IdentityResult setAuthTokenResult = await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook, ApiConstants.DataTokenProviders.ExternalLoginProviders.AuthToken, externalUser.AuthToken);
                    bool setSucceeded = setAuthTokenResult.Succeeded;

                    _logger.LogDebug("Was Facebook auth token successfully set: {setSucceeded}.", setSucceeded);

                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser, _userManager, _roleManager);

                    accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);
                }
                else
                {
                    _logger.LogError("External provider token was invalid.");

                    return Unauthorized_InvalidExternalProviderToken();
                }
            }
            // Sign user up
            else
            {
                _logger.LogInformation("User does not exist.");

                HttpResponseMessage response;
                using (HttpClient client = new HttpClient())
                {
                    Uri verifyFacebookTokenUrl = new Uri(string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}", externalUser.AuthToken, _configuration["Facebook:ClientId"], _configuration["Facebook:ClientSecret"]));
                    response = await client.GetAsync(verifyFacebookTokenUrl);
                }

                if (response.IsSuccessStatusCode == true)
                {
                    appUser = new ApplicationUser
                    {
                        Email = externalUser.Email,
                        FirstName = externalUser?.FirstName,
                        LastName = externalUser?.LastName,
                        UserName = externalUser.Email,
                        ExternalProvider = externalUser?.Provider,
                        PhotoUrl = externalUser?.PhotoUrl,
                        ExternalProviderUserId = externalUser?.Id
                    };

                    PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
                    string password = passwordHasher.HashPassword(appUser, Guid.NewGuid().ToString());

                    IdentityResult result = await _userManager.CreateAsync(appUser, password);

                    if (result.Succeeded == false)
                    {
                        _logger.LogError("User registration data is invalid or missing.");

                        return BadRequest_UserNotCreated(result.Errors);
                    }

                    ApplicationUser signedupExternalUser = await _userManager.FindByEmailAsync(appUser.Email);

                    if (signedupExternalUser == null)
                    {
                        _logger.LogError($"Unable to find user by email: { signedupExternalUser.Email }. Attempted to retrieve newly registered user to generate access token.");

                        return BadRequest_UserRegistrationError();
                    }

                    // persist auth token
                    IdentityResult setAuthTokenResult = await _userManager.SetAuthenticationTokenAsync(signedupExternalUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook, ApiConstants.DataTokenProviders.ExternalLoginProviders.AuthToken, externalUser.AuthToken);
                    bool setSucceeded = setAuthTokenResult.Succeeded;

                    _logger.LogDebug("Was Facebook auth token successfully set: {setSucceeded}.", setSucceeded);

                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupExternalUser, _userManager, _roleManager);

                    accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupExternalUser.Id, claimsIdentity);
                }
                else
                {
                    _logger.LogError("External provider token was invalid.");

                    return Unauthorized_InvalidExternalProviderToken();
                }
            }

            return Ok(accessToken);
        }

    }
}
