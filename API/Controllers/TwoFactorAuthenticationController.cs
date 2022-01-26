using API.Authentication;
using Domain;
using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Two factor authentication controller.
    /// </summary>
    [ApiController]
    [Route("2fa")]
    public class TwoFactorAuthenticationController : ApiControllerBase
    {
        public TwoFactorAuthenticationController(IConfiguration configuration,
            BotLeadslyUserManager userManager,            
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            UrlEncoder urlEncoder,
            ILogger<TwoFactorAuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;        
            _urlEncoder = urlEncoder;                        
            _logger = logger;
            _claimsIdentityService = claimsIdentityService;
            _tokenService = tokenService;
        }

        private readonly IConfiguration _configuration;
        private readonly IAccessTokenService _tokenService;
        private readonly IClaimsIdentityService _claimsIdentityService;
        private readonly BotLeadslyUserManager _userManager;        
        private readonly UrlEncoder _urlEncoder;        
        private readonly ILogger<TwoFactorAuthenticationController> _logger;

        /// <summary>
        /// Disables two factor authentication.
        /// </summary>
        /// <returns></returns>
        [HttpPost]        
        [Route("disable")]
        public async Task<IActionResult> Disable2fa()
        {
            _logger.LogTrace("Disable2fa action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);            
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
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            // return disable 2FA result
            return NoContent();
        }

        /// <summary>
        /// Resets authenticator.
        /// </summary>
        /// <returns></returns>
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
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return NoContent();
        }

        /// <summary>
        /// Signs user in, by redeeming their two factor authentication recovery code.
        /// </summary>
        /// <param name="recoveryCode"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("redeem-recovery-code")]
        public async Task<IActionResult> RedeemRecoveryCode([FromBody] TwoFactorAuthenticationBackupCodeViewModel recoveryCode)
        {
            _logger.LogTrace("RedeemBackupCode action executed.");

            ApplicationUser appUser = await _userManager.FindByEmailAsync(recoveryCode.Email);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return BadRequest_UserNotFound();
            }

            string verificationCode = recoveryCode.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            IdentityResult isRecoveryCodeValid = await _userManager.RedeemTwoFactorRecoveryCodeAsync(appUser, verificationCode);

            if (isRecoveryCodeValid.Succeeded == false)
            {
                _logger.LogDebug("Recovery code was not valid.");

                return BadRequest_RecoveryCodeIsNotValid();
            }

            // if user successfully logged in reset access failed count back to zero.
            await _userManager.ResetAccessFailedCountAsync(appUser);

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(accessToken);
        }

        /// <summary>
        /// Verifies two step verification code entered by user and provider sent by the client application.
        /// </summary>
        /// <param name="twoFactorVerification"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("verify-two-step-verification-code")]
        public async Task<IActionResult> VerifyTwoStepVerificationCode([FromBody] TwoFactorAuthenticationVerificationCodeViewModel twoFactorVerification)
        {
            _logger.LogTrace("VerifyTwoStepVerificationCode action executed.");

            ApplicationUser appUser = await _userManager.FindByEmailAsync(twoFactorVerification.Email);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return BadRequest_UserNotFound();
            }

            string verificationCode = twoFactorVerification.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(appUser, twoFactorVerification.Provider, verificationCode);

            if (is2faTokenValid == false)
            {
                _logger.LogDebug("Verification code or provider was not valid.");

                return BadRequest_TwoStepVerificationCodeOrProviderIsInvalid();
            }

            // if user successfully logged in reset access failed count back to zero.
            await _userManager.ResetAccessFailedCountAsync(appUser);

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(accessToken);
        }

        /// <summary>
        /// Generates new recovery codes.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("generate-recovery-codes")]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            _logger.LogTrace("GenerateRecoveryCodes action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(appUser);            
            if (isTwoFactorEnabled == false)
            {
                _logger.LogDebug("Two factor authentication is not enabled.");
                return BadRequest_TwoFactorAuthenticationIsNotEnabled();
            }
                        
            UserRecoveryCodesViewModel recoveryCodes = new UserRecoveryCodesViewModel
            {
                Items = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(appUser, ApiConstants.TwoFactorAuthentication.NumberOfRecoveryCodes)
            };

            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(recoveryCodes);
        }

        /// <summary>
        /// Verifies authenticator setup code and enables two factor authentication.
        /// </summary>
        /// <param name="verifyAuthenticatorCode"></param>
        /// <returns></returns>
        [HttpPost]        
        [Route("verify-authenticator")]
        public async Task<IActionResult> VerifyAuthenticator([FromBody] TwoFactorAuthenticationVerificationCodeViewModel verifyAuthenticatorCode)
        {
            _logger.LogTrace("VerifyAuthenticator action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
                       
            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return BadRequest_UserNotFound();
            }

            string verificationCode = verifyAuthenticatorCode.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(appUser, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2faTokenValid == false)
            {
                _logger.LogDebug("Verification code was not valid.");

                return BadRequest_SetupVerificationCodeIsInvalid();
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
                    Items = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(appUser, ApiConstants.TwoFactorAuthentication.NumberOfRecoveryCodes)
                }
            };

            // reset user stay signed in token, because once we disable two factor auth, user time stamps will not match
            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(model);
        }        

        /// <summary>
        /// Gets authenticator setup key and QR code.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("setup-authenticator")]
        public async Task<IActionResult> SetupAuthenticator()
        {
            _logger.LogTrace("SetupAuthenticator action executed.");

            ApplicationUser user = await _userManager.GetUserAsync(User);
            AuthenticatorSetupViewModel authenticatorSetupDetails = await GetAuthenticatorDetailsAsync(user);

            return Ok(authenticatorSetupDetails);
        }

        /// <summary>
        /// Gets authenticator setup details.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates QR code uri.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="unformattedKey"></param>
        /// <returns></returns>
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            _logger.LogTrace("Generating Qr code uri.");

            string appDisplayName = _configuration.GetValue<string>("TwoFactorAuthDisplayAppName");

            if (string.IsNullOrEmpty(appDisplayName))
            {
                _logger.LogDebug("Check appsettings.json file. 'TwoFactorAuthDisplayAppName' key value pair is not properly set.");

                appDisplayName = "bot-leadsly-dot-net-api-starter";
            }

            return string.Format(
                ApiConstants.TwoFactorAuthentication.AuthenticatorUriFormat,
                _urlEncoder.Encode(appDisplayName),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        /// <summary>
        /// Formats authenticator setup shared key.
        /// </summary>
        /// <param name="unformattedKey"></param>
        /// <returns></returns>
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
