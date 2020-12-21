using Domain;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;

namespace API.Services
{
    public class EmailService : IEmailService
    {
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _logger = logger;
        }

        private IConfigurationSection _emailServiceOptions;
        private IConfiguration _configuration;
        private ILogger<EmailService> _logger;

        public bool SendEmail(MimeMessage message)
        {
            bool succeeded;

            try
            {
                _logger.LogInformation("Preparing to send email.");

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(_emailServiceOptions[nameof(EmailServiceOptions.SmtpServer)], int.Parse(_emailServiceOptions[nameof(EmailServiceOptions.Port)]), true);

                    client.Authenticate(_emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)], _configuration[ApiConstants.VaultKeys.SystemAdminEmailPassword]);

                    client.Send(message);

                    client.Disconnect(true);
                }

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured while sending email.", ex);

                succeeded = false;
            }

            return succeeded;
        }

        public MimeMessage ComposeEmail(ComposeEmailSettingsModel settings)
        {
            // Message to User
            MimeMessage messageToUser = new MimeMessage();

            // Add From
            messageToUser.From.Add(settings.From);            
            // Add TO            
            messageToUser.To.Add(settings.From);

            // Add Subject            
            messageToUser.Subject = settings.Subject;

            messageToUser.Priority = MessagePriority.Urgent;

            BodyBuilder bodyBuilderForUser = new BodyBuilder
            {
                HtmlBody = settings.Body
            };

            messageToUser.Body = bodyBuilderForUser.ToMessageBody();

            return messageToUser;
        }
    }
}
