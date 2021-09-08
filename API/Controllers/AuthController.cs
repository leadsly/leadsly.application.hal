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

namespace API.Controllers
{
    /// <summary>
    /// Authentication controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ApiControllerBase
    {
        public AuthController(
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            IConfiguration configuration,
            OdmUserManager userManager,
            UrlEncoder urlEncoder,
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _claimsIdentityService = claimsIdentityService;
            _userManager = userManager;
            _configuration = configuration;
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
        private readonly OdmUserManager _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Signs user in.
        /// </summary>
        /// <param name="signin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninUserViewModel signin, CancellationToken ct = default)
        {
            _logger.LogTrace("Signin action executed.");

            if (string.IsNullOrEmpty(signin.Password))
            {
                _logger.LogDebug("Request was missing user's password.");

                return Unauthorized_InvalidCredentials();
            }

            ApplicationUser appUser = await _userManager.FindByEmailAsync(signin.Email);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return Unauthorized_InvalidCredentials();
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            if (await _userManager.IsEmailConfirmedAsync(appUser) == false)
            {
                //[CONFIRMATION-WALL]: Keep code if email confirmation is required.
                //await RequireConfirmedEmail(appUser);
            }

            if (await _userManager.CheckPasswordAsync(appUser, signin.Password) == false)
            {
                _logger.LogDebug("Password validation failed.");

                await _userManager.AccessFailedAsync(appUser);

                if (await _userManager.IsLockedOutAsync(appUser))
                {
                    _logger.LogDebug("User is locked out.");

                    return Unauthorized_AccountLockedOut();
                }

                int failedAttempts = await _userManager.GetAccessFailedCountAsync(appUser);

                _logger.LogDebug("Failed sign in attempt number: '{failedAttempts}'.", failedAttempts);

                return Unauthorized_InvalidCredentials(failedAttempts);
            }

            if (await _userManager.GetTwoFactorEnabledAsync(appUser) == true)
            {
                var providers = await _userManager.GetValidTwoFactorProvidersAsync(appUser);
                if (providers.Contains("Authenticator") == false)
                {
                    return BadRequest_TwoFactorAuthenticationIsNotEnabled();
                }

                return Ok(new AuthResponseViewModel
                {
                    Is2StepVerificationRequired = true,
                    Provider = "Authenticator"
                });
            }

            // if user successfully logged in reset access failed count back to zero.
            await _userManager.ResetAccessFailedCountAsync(appUser);

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(new AuthResponseViewModel
            {
                AccessToken = accessToken
            });
        }

        /// <summary>
        /// Signs user up.
        /// </summary>
        /// <param name="signupModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupUserViewModel signupModel, CancellationToken ct = default)
        {
            _logger.LogTrace("Signup action executed.");

            if (string.Equals(signupModel.Password, signupModel.ConfirmPassword, StringComparison.OrdinalIgnoreCase) == false)
            {
                _logger.LogDebug("Confirm password and password do not match.");

                return BadRequest_UserRegistrationError();
            }

            ApplicationUser appUser = new ApplicationUser
            {
                Email = signupModel.Email,
                UserName = signupModel.Email,
                ApplicationId = $"{Guid.NewGuid()}"
            };

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            IdentityResult result = await _userManager.CreateAsync(appUser, signupModel.Password);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("User registration data is invalid or missing.");

                return BadRequest_UserNotCreated(result.Errors);
            }

            // create and send out email confirmation
            string confirmEmailToken = await this._userManager.GenerateEmailConfirmationTokenAsync(appUser);

            if (confirmEmailToken == null)
            {
                _logger.LogDebug("Signup action failed to generate email confirmation token.");

                return BadRequest_FailedToGenerateToken();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(confirmEmailToken);

            IConfigurationSection clientOptions = this._configuration.GetSection(nameof(ClientOptions));
            string callBackUrl = ApiConstants.Email.Verify.Url.Replace(ApiConstants.Email.ClientAddress, clientOptions[nameof(ClientOptions.Address)]);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.IdParam, appUser.Id);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.EmailParam, appUser.Email);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.TokenParam, token);

            IConfigurationSection emailServiceOptions = this._configuration.GetSection(nameof(EmailServiceOptions));
            IConfigurationSection verifyEmail = emailServiceOptions.GetSection(nameof(EmailServiceOptions.VerifyEmail));
            ComposeEmailSettingsModel settings = new ComposeEmailSettingsModel
            {
                Subject = verifyEmail[nameof(EmailServiceOptions.VerifyEmail.EmailSubject)],
                To = new MailboxAddress(appUser.Email, "omikolaj1@gmail.com"), //appUser.Email),
                From = new MailboxAddress(emailServiceOptions[nameof(EmailServiceOptions.SystemAdminName)], emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)]),
                Body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.VerifyEmail)
            };

            settings.Body = settings.Body.Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

            MimeMessage message = _emailService.ComposeEmail(settings);

            string email = appUser.Email;
            if (_emailService.SendEmail(message))
            {
                _logger.LogInformation("Email verification has been sent to: '{email}'", email);
                // if you don't want to sign user in after signing up add return NoContent();                
            }
            else
            {
                _logger.LogInformation("Email verification has failed to send to: '{email}'", email);
                // if you don't want to sign user in after signing up add return BadRequest_FailedToSendConfirmationEmail();
            }

            // if you dont want to sign user in after signing up, comment out rest of this code.            
            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            // TODO may cause issues if StaySignedIn token was not previously set. NEEDS TO BE TESTED
            _logger.LogWarning("NEEDS TO BE TESTED. IF NEW USER DOES NOT HAVE STAYSIGNEDIN TOKEN DOES CALLING RemoveAuthenticationTokenAsync CAUSE ISSUES?");
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(accessToken);
        }

        /// <summary>
        /// Refreshes user's authentication token.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            _logger.LogTrace("RefreshToken action executed.");

            RenewAccessTokenResultViewModel result = new RenewAccessTokenResultViewModel();

            string expiredAccessToken = HttpContext.GetAccessToken();

            throw new Exception();

            if (expiredAccessToken == string.Empty)
            {
                _logger.LogWarning("Expired access token not present on the reqest.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            try
            {
                // request has token but it failed authentication. Attempt to renew the token
                result = await _tokenService.TryRenewAccessToken(expiredAccessToken);
                bool succeeded = result.Succeeded;
                _logger.LogDebug("Attempted to rewnew jwt. Result: {succeeded}.", succeeded);                
            }
            catch (Exception ex)
            {
                // Silently fail
                _logger.LogError(ex, "Failed to rewnew jwt.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            if (result.Succeeded == false)
            {
                _logger.LogError("Failed to rewnew jwt.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            return Ok(result.AccessToken);
        }

        /// <summary>
        /// Signs user in using Google's sign in.
        /// </summary>
        /// <param name="externalUser"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("external-signin-google")]
        public async Task<IActionResult> ExternalSigninGoogle([FromBody] SocialUserViewModel externalUser)
        {
            _logger.LogTrace("ExternalSigninGoogle action executed.");
            ApplicationUser appUser = await _userManager.FindByEmailAsync(externalUser.Email);

            ApplicationAccessTokenViewModel accessToken;

            // Sign user in
            if (appUser != null)
            {
                bool result = await _userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Google, ApiConstants.DataTokenProviders.ExternalLoginProviders.IdToken, externalUser.IdToken);

                if (result == true)
                {
                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

                    accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);
                }
                else
                {
                    _logger.LogDebug("External provider token was invalid.");

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
                    ExternalProviderUserId = externalUser.Id,
                    ApplicationId = $"{Guid.NewGuid()}"
                };

                PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
                string password = passwordHasher.HashPassword(appUser, Guid.NewGuid().ToString());

                IdentityResult result = await _userManager.CreateAsync(appUser, password);

                if (result.Succeeded == false)
                {
                    _logger.LogDebug("User registration data is invalid or missing.");

                    return BadRequest_UserNotCreated(result.Errors);
                }

                ApplicationUser signedupExternalUser = await _userManager.FindByEmailAsync(appUser.Email);

                if (signedupExternalUser == null)
                {
                    _logger.LogDebug($"Unable to find user by email: { signedupExternalUser.Email }. Attempted to retrieve newly registered user to generate access token.");

                    return BadRequest_UserRegistrationError();
                }

                ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupExternalUser);

                accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupExternalUser.Id, claimsIdentity);
            }

            // remove old refresh token if any where present
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(accessToken);
        }

        /// <summary>
        /// Signs user in using Facebook's sign in.
        /// </summary>
        /// <param name="externalUser"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("external-signin-facebook")]
        public async Task<IActionResult> ExternalSigninFacebook([FromBody] SocialUserViewModel externalUser)
        {
            _logger.LogTrace("ExternalSigninFacebook action executed.");

            ApplicationAccessTokenViewModel accessToken;

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

                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

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
                    Uri verifyFacebookTokenUrl = new Uri(string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}", externalUser.AuthToken, _configuration[ApiConstants.VaultKeys.FaceBookClientId], _configuration[ApiConstants.VaultKeys.FaceBookClientSecret]));
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
                        ExternalProviderUserId = externalUser?.Id,
                        ApplicationId = $"{Guid.NewGuid()}"
                    };

                    PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
                    string password = passwordHasher.HashPassword(appUser, Guid.NewGuid().ToString());

                    IdentityResult result = await _userManager.CreateAsync(appUser, password);

                    if (result.Succeeded == false)
                    {
                        _logger.LogDebug("User registration data is invalid or missing.");

                        return BadRequest_UserNotCreated(result.Errors);
                    }

                    ApplicationUser signedupExternalUser = await _userManager.FindByEmailAsync(appUser.Email);

                    if (signedupExternalUser == null)
                    {
                        _logger.LogDebug($"Unable to find user by email: { signedupExternalUser.Email }. Attempted to retrieve newly registered user to generate access token.");

                        return BadRequest_UserRegistrationError();
                    }

                    // persist auth token
                    IdentityResult setAuthTokenResult = await _userManager.SetAuthenticationTokenAsync(signedupExternalUser, ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook, ApiConstants.DataTokenProviders.ExternalLoginProviders.AuthToken, externalUser.AuthToken);
                    bool setSucceeded = setAuthTokenResult.Succeeded;

                    _logger.LogDebug("Was Facebook auth token successfully set: {setSucceeded}.", setSucceeded);

                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupExternalUser);

                    accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupExternalUser.Id, claimsIdentity);
                }
                else
                {
                    _logger.LogDebug("External provider token was invalid.");

                    return Unauthorized_InvalidExternalProviderToken();
                }
            }

            // remove old refresh token if any where present
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(accessToken);
        }

        /// <summary>
        /// Signs user out.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [AllowAnonymous]
        [Route("signout")]
        public async Task<IActionResult> Signout(CancellationToken ct = default)
        {
            _logger.LogTrace("Signout action executed.");

            Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                ApplicationUser appUser = await _userManager.FindByIdAsync(userIdClaim.Value);

                if (appUser != null)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Requires that user's email is confirmed before signing in.
        /// </summary>
        /// <param name="appUser"></param>
        /// <returns></returns>
        private async Task<IActionResult> RequireConfirmedEmail(ApplicationUser appUser)
        {
            _logger.LogDebug("User's email is not confirmed. Unable to log user in.");

            // attempt to resend email confirmation
            string confirmEmailToken = await this._userManager.GenerateEmailConfirmationTokenAsync(appUser);

            if (confirmEmailToken == null)
            {
                _logger.LogDebug("Signin action failed to generate email confirmation token.");

                return BadRequest_FailedToGenerateToken();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(confirmEmailToken);

            IConfigurationSection clientOptions = this._configuration.GetSection(nameof(ClientOptions));
            string callBackUrl = ApiConstants.Email.Verify.Url.Replace(ApiConstants.Email.ClientAddress, clientOptions[nameof(ClientOptions.Address)]);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.EmailParam, appUser.Email);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.TokenParam, token);

            IConfigurationSection emailServiceOptions = this._configuration.GetSection(nameof(EmailServiceOptions));
            IConfigurationSection verifyEmail = emailServiceOptions.GetSection(nameof(EmailServiceOptions.VerifyEmail));
            ComposeEmailSettingsModel settings = new ComposeEmailSettingsModel
            {
                Subject = verifyEmail[nameof(EmailServiceOptions.VerifyEmail.EmailSubject)],
                To = new MailboxAddress(appUser.Email, appUser.Email),
                From = new MailboxAddress(emailServiceOptions[nameof(EmailServiceOptions.SystemAdminName)], emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)]),
                Body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.VerifyEmail)
            };

            settings.Body = settings.Body.Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

            MimeMessage message = _emailService.ComposeEmail(settings);

            string email = appUser.Email;
            if (_emailService.SendEmail(message))
            {
                _logger.LogInformation("Email verification has been sent to: '{email}'", email);
            }
            else
            {
                _logger.LogInformation("Email verification has failed to send to: '{email}'", email);
            }

            return Forbidden_EmailNotConfirmed();
        }
    }
}
