using MimeKit;

namespace API.Services
{
    public interface IEmailService
    {
        bool SendEmail(MimeMessage message);

        MimeMessage ComposeEmail(ComposeEmailSettingsModel settings);
    }
}
