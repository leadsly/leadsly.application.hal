namespace API.Services
{
    public class EmailServiceOptions
    {
        public string SystemAdminName { get; set; }                
        public string Port { get; set; }
        public string SmtpServer { get; set; }
        public string SystemAdminEmail { get; set; }

        public class PasswordReset
        {
            public string EmailSubject { get; set; }
        }

        public class VerifyEmail
        {
            public string EmailSubject { get; set; }
        }

        public class ChangeEmail
        {
            public string EmailSubject { get; set; }
        }
    }
}
