using Domain;
using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("2fa")]
    public class TwoFactorAuthenticationController : ApiControllerBase
    {
        public TwoFactorAuthenticationController(IConfiguration configuration,
            OdmUserManager userManager,            
            IHtmlTemplateGenerator templateGenerator,
            UrlEncoder urlEncoder,
            ILogger<TwoFactorAuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;            
            _templateGenerator = templateGenerator;
            _urlEncoder = urlEncoder;                        
            _logger = logger;
        }

        private readonly IConfiguration _configuration;
        private readonly OdmUserManager _userManager;        
        private readonly UrlEncoder _urlEncoder;        
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<TwoFactorAuthenticationController> _logger;

        [HttpPost]        
        [Route("disable")]
        public async Task<IActionResult> Disable2fa()
        {
            _logger.LogTrace("Disable2fa action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            var a = true;
            if (a)
            {

                throw new System.Exception();
            }

            if(appUser == null)
            {
                _logger.LogDebug("User not found or does not exist.");

                return BadRequest_UserNotFound();
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(appUser);

            if (isTwoFactorEnabled == false)
            {
                _logger.LogDebug("Two factor authentication is not setup, thus cannot be disabled.");

                return BadRequest_CannotDisable2faWhenItsNotEnabled();                
            }

            // this operation changes user's security time stamp, custom token will have to be re-issued.
            IdentityResult result = await _userManager.SetTwoFactorEnabledAsync(appUser, false);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Disabling two factor authentication encountered a problem when executing 'SetTwoFactorEnabledAsync(user, false)'.");

                return BadRequest_FailedToDisable2fa();
            }

            // reset user stay signed in token, because once we disable two factor auth, user time stamps will not match
            await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);

            string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose);

            await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName, refreshToken);

            // return disable 2FA result
            return NoContent();
        }

        [HttpPost]
        [Route("reset-authenticator")]
        public async Task<IActionResult> ResetAuthenticator()
        {
            _logger.LogTrace("ResetAuthenticator action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            if(appUser == null)
            {
                _logger.LogDebug("User not found.");

                return BadRequest_UserNotFound();
            }

            // this operation changes user's security time stamp, custom token will have to be re-issued.
            IdentityResult disableTwoFactorAuthResult = await _userManager.SetTwoFactorEnabledAsync(appUser, false);

            if(disableTwoFactorAuthResult.Succeeded == false)
            {
                _logger.LogDebug("Disabling two factor authentication encountered a problem when executing 'SetTwoFactorEnabledAsync(user, false)'.");

                return BadRequest_FailedToDisable2fa();
            }

            IdentityResult resetAuthenticatorKeysResult = await _userManager.ResetAuthenticatorKeyAsync(appUser);            

            if(resetAuthenticatorKeysResult.Succeeded == false)
            {
                _logger.LogDebug("Resetting two factor authenticator key encountered a problem when executing 'ResetAuthenticatorKeyAsync(user)'.");

                return BadRequest_FailedToResetAuthenticatorKey();
            }

            // reset user stay signed in token, because once we disable two factor auth, user time stamps will not match
            await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);

            string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose);

            await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName, refreshToken);

            return NoContent();
        }

        [HttpPost]
        [Route("generate-recovery-codes")]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            _logger.LogTrace("GenerateRecoveryCodes action executed.");

            var a = true;
            if (a)
            {
                throw new System.Exception();
            }            

            ApplicationUser user = await _userManager.GetUserAsync(User);

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            isTwoFactorEnabled = false;
            if (isTwoFactorEnabled == false)
            {
                return BadRequest_TwoFactorAuthenticationIsNotEnabled();
            }
                        
            UserRecoveryCodesViewModel recoveryCodes = new UserRecoveryCodesViewModel
            {
                Items = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)
            };

            return Ok(recoveryCodes);
        }

        [HttpPost]        
        [Route("verify-authenticator")]
        public async Task<IActionResult> VerifyAuthenticator([FromBody] string verifyAuthenticatorCode)
        {
            _logger.LogTrace("VerifyAuthenticator action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
           
            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return BadRequest_UserNotFound();
            }

            string verificationCode = verifyAuthenticatorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(appUser, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2faTokenValid == false)
            {
                _logger.LogDebug("Verification code was not valid.");

                return BadRequest_VerificationCodeIsInvalid();
            }

            // this operation changes user's security time stamp, custom token will have to be re-issued.
            IdentityResult result = await _userManager.SetTwoFactorEnabledAsync(appUser, true);

            if(result.Succeeded == false)
            {
                _logger.LogDebug("Enabling two factor authentication encountered a problem when executing 'SetTwoFactorEnabledAsync(user, true)'.");

                return BadRequest_FailedToEnable2fa();
            }

            AuthenticatorSetupResultViewModel model = new AuthenticatorSetupResultViewModel
            {
                Status = TwoFactorAuthenticationStatus.Succeeded,
                RecoveryCodes = new UserRecoveryCodesViewModel
                {
                    Items = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(appUser, 10)
                }
            };

            // reset user stay signed in token, because once we disable two factor auth, user time stamps will not match
            await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);

            string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose);

            await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName, refreshToken);

            return Ok(model);
        }

        [HttpGet]
        [Route("setup-authenticator")]
        public async Task<IActionResult> SetupAuthenticator()
        {
            _logger.LogTrace("SetupAuthenticator action executed.");

            var a = true;
            if (a)
            {
                throw new System.Exception();
            }

            ApplicationUser user = await _userManager.GetUserAsync(User);
            AuthenticatorSetupViewModel authenticatorSetupDetails = await GetAuthenticatorDetailsAsync(user);

            return Ok(authenticatorSetupDetails);
        }

        private async Task<AuthenticatorSetupViewModel> GetAuthenticatorDetailsAsync(ApplicationUser user)
        {
            // load the authenticator key and & QR code URI to display on the form
            string unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            string email = await _userManager.GetEmailAsync(user);

            return new AuthenticatorSetupViewModel
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey)
            };
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            _logger.LogTrace("Generating Qr code uri.");

            string appDisplayName = _configuration.GetValue<string>("TwoFactorAuthDisplayAppName");

            if (string.IsNullOrEmpty(appDisplayName))
            {
                _logger.LogError("Check appsettings.json file. 'TwoFactorAuthDisplayAppName' key value pair is not properly set.");

                appDisplayName = "odiam-dot-net-api-starter";
            }

            return string.Format(
                ApiConstants.TwoFactorAuthentication.AuthenticatorUriFormat,
                _urlEncoder.Encode(appDisplayName),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            _logger.LogTrace("Formatting two factor authentication setup key.");

            StringBuilder result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }
    }
}
