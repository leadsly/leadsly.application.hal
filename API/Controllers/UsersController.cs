﻿using API.Services;
using Domain;
using Domain.Models;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Users controller.
    /// </summary>
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

        /// <summary>
        /// Gets users account security details.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/security")]
        public async Task<IActionResult> SecurityDetails()
        {
            _logger.LogTrace("Security details action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                // return bad request user not found
                return BadRequest_UserNotFound();
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(appUser);

            string recoveryCodeString = await _userManager.GetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.AspNetUserProvider.ProviderName, ApiConstants.DataTokenProviders.AspNetUserProvider.TokenName);
            UserRecoveryCodesViewModel recoveryCodes = new UserRecoveryCodesViewModel
            {
                Items = recoveryCodeString?.Split(';') ?? Array.Empty<string>()
            };

            AccountSecurityDetailsViewModel securityDetails = new AccountSecurityDetailsViewModel
            {
                ExternalLogins = logins.Select(l => l.ProviderDisplayName).ToList(),
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(appUser) != null,
                RecoveryCodes = recoveryCodes,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(appUser),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(appUser)
            };

            return Ok(securityDetails);
        }

        /// <summary>
        /// Gets users account general details.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/general")]
        public async Task<IActionResult> GeneralDetails()
        {
            _logger.LogTrace("General details action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);            
            if(appUser == null)
            {
                _logger.LogDebug("User not found.");

                // return bad request user not found
                return BadRequest_UserNotFound();
            }

            AccountGeneralDetailsViewModel generalDetails = new AccountGeneralDetailsViewModel
            {
                Email = appUser.Email,
                Verified = appUser.EmailConfirmed
            };

            return Ok(generalDetails);
        }

        /// <summary>
        /// Resends email confirmation email.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/resend-email-verification")]
        public async Task<IActionResult> ResendEmailVerification()
        {
            _logger.LogTrace("Resend email verification action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            if(appUser == null)
            {
                // return bad request user not found
                return BadRequest_UserNotFound();
            }

            // attempt to resend email here;

            return NoContent();
        }

        /// <summary>
        /// Checks if email has already been registered.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("email")]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            _logger.LogTrace("CheckIfEmailExists action executed. Looking for: {email}", email);

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            return user == null ? new JsonResult(false) : new JsonResult(true);
        }

        /// <summary>
        /// Updates user's password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [AllowAnonymous]
        [Route("password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModelViewModel model)
        {
            _logger.LogTrace("ResetPassword action executed.");

            ApplicationUser userToResetPassword = await _userManager.FindByIdAsync(model.UserId);

            if(userToResetPassword == null)
            {
                string userId = model.UserId;
                _logger.LogDebug("ResetPassword action failed. Unable to find by provided userId: {userId}.", userId);

                return BadRequest_FailedToUpdatePassword();
            }

            if(model.PasswordResetToken == null)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset token not found in the request.");

                return BadRequest_FailedToUpdatePassword();
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(userToResetPassword, model.PasswordResetToken, model.Password);

            if(result.Succeeded == false)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset operation failed to update the new password.");

                return BadRequest_FailedToUpdatePassword();
            }

            return NoContent();
        }

        /// <summary>
        /// Emails user a password reset link.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            // TODO consider adding client id to all of the calls to allow for single backend api and multiple client apps

            _logger.LogTrace("ForgotPassword action executed. Generating reset password link for: '{email}'", email);

            ApplicationUser userToRecoverPassword = await _userManager.FindByEmailAsync(email);

            // We want to fail silently
            if (userToRecoverPassword == null)
            {
                _logger.LogDebug("User not found.");
                // TODO consider sending NoContent()
                return Ok();
            }

            string passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(userToRecoverPassword);

            if (passwordResetCode == null)
            {
                _logger.LogDebug("Failed to generate password reset token");
                // TODO consider sending NoContent()
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
                _logger.LogInformation("Password recovery email has been sent to: '{email}'", email);
                // TODO consider sending NoContent()
                return Ok();
            }

            return BadRequest_FailedToSendEmail();
        }
    }
}
