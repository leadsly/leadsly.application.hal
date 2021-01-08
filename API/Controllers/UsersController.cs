using API.Authentication;
using API.Services;
using Domain;
using Domain.Models;
using Domain.ViewModels;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ApiControllerBase
    {
        public UsersController(
            IConfiguration configuration,
            OdmUserManager userManager,
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            SignInManager<ApplicationUser> signinManager,
            UrlEncoder urlEncoder,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _templateGenerator = templateGenerator;
            _urlEncoder = urlEncoder;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _signinManager = signinManager;
            _logger = logger;
        }

        private readonly IConfiguration _configuration;
        private readonly OdmUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<UsersController> _logger;

        [HttpGet]
        [Route("{id}/account/security")]
        public async Task<IActionResult> SecurityDetails()
        {
            _logger.LogTrace("Security details action executed.");

            ApplicationUser user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                // return bad request user not found
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            string recoveryCodeString = await _userManager.GetAuthenticationTokenAsync(user, ApiConstants.AspNetUserTokens.AspNetUserStore_LoginProvider, ApiConstants.AspNetUserTokens.Name);
            UserRecoveryCodesViewModel recoveryCodes = new UserRecoveryCodesViewModel
            {
                Items = recoveryCodeString?.Split(';') ?? Array.Empty<string>()
            };

            AccountSecurityDetailsViewModel securityDetails = new AccountSecurityDetailsViewModel
            {
                ExternalLogins = logins.Select(l => l.ProviderDisplayName).ToList(),
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                RecoveryCodes = recoveryCodes,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };

            return Ok(securityDetails);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Details()
        {
            _logger.LogTrace("Details action executed.");

            ApplicationUser user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // return Bad request
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);

            AccountDetailsViewModel profileDetails = new AccountDetailsViewModel
            {
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                ExternalLogins = logins.Select(l => l.ProviderDisplayName).ToList(),
                TwoFactorClientRemembered = false,                
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };

            return Ok(profileDetails);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("email")]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            _logger.LogTrace("CheckIfEmailExists action executed. Looking for: {email}", email);

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            return user == null ? new JsonResult(false) : new JsonResult(true);
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModelViewModel model)
        {
            _logger.LogTrace("ResetPassword action executed.");

            ApplicationUser userToResetPassword = await _userManager.FindByEmailAsync(model.Email);

            if(userToResetPassword == null)
            {
                string email = model.Email;
                _logger.LogDebug("ResetPassword action failed. Unable to find by provided email: {email}.", email);

            }

            if(model.PasswordResetToken == null)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset token not found in the request.");
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(userToResetPassword, model.PasswordResetToken, model.Password);

            if(result.Succeeded == false)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset operation failed to update the new password.");
            }

            return NoContent();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            // TODO consider adding client id to all of the calls to allow for single backend api and multiple client apps

            _logger.LogTrace("ForgotPassword action executed. Generating reset password link for {email}", email);

            ApplicationUser userToRecoverPassword = await _userManager.FindByEmailAsync(email);

            // We want to fail silently
            if (userToRecoverPassword == null)
            {
                return Ok();
            }

            string passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(userToRecoverPassword);

            if (passwordResetCode == null)
            {
                return Ok();
            }

            string code = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(passwordResetCode);

            string callBackUrl = $"http://localhost:4200/auth/reset-password?userId={userToRecoverPassword.Id}&code={code}";

            ComposeEmailSettingsModel settings = new ComposeEmailSettingsModel
            {
                Subject = "Password Recovery",
                To = new MailboxAddress(_emailServiceOptions[nameof(EmailServiceOptions.SystemAdminName)], _emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)]),
                From = new MailboxAddress("System Admin", email),
                Body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.PasswordReset)
            };

            settings.Body = settings.Body.Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

            MimeMessage message = _emailService.ComposeEmail(settings);

            if (_emailService.SendEmail(message))
            {
                return Ok();
            }

            return BadRequest_FailedToSendEmail();
        }
    }
}
