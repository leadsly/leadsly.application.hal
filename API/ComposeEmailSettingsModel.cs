using MimeKit;

namespace API
{
    public class ComposeEmailSettingsModel
    {
        public MailboxAddress To { get; set; }
        public MailboxAddress From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
