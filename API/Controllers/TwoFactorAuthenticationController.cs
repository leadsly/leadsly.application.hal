using Domain;
using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            SignInManager<ApplicationUser> signinManager,
            UrlEncoder urlEncoder,
            ILogger<TwoFactorAuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;            
            _templateGenerator = templateGenerator;
            _urlEncoder = urlEncoder;            
            _signinManager = signinManager;
            _logger = logger;
        }

        private readonly IConfiguration _configuration;
        private readonly OdmUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly UrlEncoder _urlEncoder;        
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<TwoFactorAuthenticationController> _logger;

        [HttpPost]        
        [Route("disable")]
        public async Task<IActionResult> Disable2FA()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                // bad request
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if (isTwoFactorEnabled == false)
            {
                // cannot disable 2FA as its not currently enabled           
                return NoContent();

                // return bad request
            }

            IdentityResult result = await _userManager.SetTwoFactorEnabledAsync(user, false);

            if(result.Succeeded == false)
            {
                // return false
                return NoContent();
            }

            // return disable 2FA result

            return NoContent();
        }

        [HttpPost]        
        [Route("generate-recovery-codes")]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            if(isTwoFactorEnabled == false)
            {

            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return new JsonResult(new
            {
                recoveryCodes = new { recoveryCodes }
            });
        }

        [HttpPost]        
        [Route("verify-authenticator")]
        public async Task<IActionResult> VerifyAuthenticator([FromBody] string verifyAuthenticator)
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {

            }

            string verificationCode = verifyAuthenticator.Replace(" ", string.Empty).Replace("-", string.Empty);

            bool is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (is2FaTokenValid == false)
            {

            }

            IdentityResult result = await _userManager.SetTwoFactorEnabledAsync(user, true);

            if(result.Succeeded == false)
            {


            }

            AuthenticatorSetupResultViewModel model = new AuthenticatorSetupResultViewModel
            {
                Status = TwoFactorAuthenticationStatus.Succeeded
            };

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                // generate recovery codes
                IEnumerable<string> recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                model.RecoveryCodes = recoveryCodes.ToList();
                
                return Ok(model);
            }

            return Ok(model);
        }

        [HttpGet]        
        [Route("setup-authenticator")]
        public async Task<IActionResult> SetupAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);            
            var authenticatorDetails = await GetAuthenticatorDetailsAsync(user);

            return new JsonResult(authenticatorDetails);
        }

        private async Task<object> GetAuthenticatorDetailsAsync(ApplicationUser user)
        {
            // load the authenticator key and & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);

            return new
            {
                sharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey)
            };
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("odiam-dotnet-api-starter"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
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
