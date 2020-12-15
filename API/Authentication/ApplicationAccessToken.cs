namespace API.Authentication
{
    public class ApplicationAccessToken
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public long expires_in { get; set; }
    }
}
