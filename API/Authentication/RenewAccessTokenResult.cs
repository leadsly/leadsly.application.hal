namespace API.Authentication
{
    public class RenewAccessTokenResult
    {
        public bool Succeeded { get; set; } = false;
        public ApplicationAccessToken AccessToken { get; set; }
    }
}
