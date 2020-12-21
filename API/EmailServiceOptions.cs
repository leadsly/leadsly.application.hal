namespace API.Services
{
    public class EmailServiceOptions
    {
        public string SystemAdminName { get; set; }        
        public string Port { get; set; }
        public string SmtpServer { get; set; }
        public string SystemAdminEmail { get; set; }
    }
}
