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

        private readonly IConfigurationSection _emailServiceOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public bool SendEmail(MimeMessage message)
        {
            bool succeeded;

            try
            {
                _logger.LogInformation("[EmailService]: Preparing to send email.");

                using (SmtpClient client = new SmtpClient())
                {
                    string smtpServer = _emailServiceOptions[nameof(EmailServiceOptions.SmtpServer)];

                    if (smtpServer == string.Empty)
                    {
                        _logger.LogError("[EmailService]: SmtpServer is not defined.");
                    }

                    string port = _emailServiceOptions[nameof(EmailServiceOptions.Port)];                    

                    if(port == string.Empty)
                    {
                        _logger.LogError("[EmailService]: Port is not defined.");
                    }

                    client.Connect(smtpServer, int.Parse(port), true);

                    string systemAdminEmail = _emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)];

                    if(systemAdminEmail == string.Empty)
                    {
                        _logger.LogError("[EmailService]: SystemAdminEmail is not defined.");
                    }

                    string systemAdminEmailPassword = _configuration[ApiConstants.VaultKeys.SystemAdminEmailPassword];

                    if(systemAdminEmailPassword == string.Empty)
                    {
                        _logger.LogError("[EmailService]: SystemAdminEmailPassword is not defined.");
                    }

                    client.Authenticate(systemAdminEmail, systemAdminEmailPassword);

                    client.Send(message);

                    client.Disconnect(true);
                }

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService]: Error occured while sending email.");

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
            
            // Add To         
            messageToUser.To.Add(settings.To);

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
