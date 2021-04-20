namespace Domain.Models
{
    public class ApplicationAccessTokenModel
    {
        public string access_token { get; set; }
        public long expires_in { get; set; }
    }
}
