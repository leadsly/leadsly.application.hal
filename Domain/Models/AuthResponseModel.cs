namespace Domain.Models
{
    public class AuthResponseModel
    {
        //public bool IsAuthSuccessful { get; set; }
        public ApplicationAccessTokenModel AccessToken { get; set; }
        public bool Is2StepVerificationRequired { get; set; }
        public string Provider { get; set; }
    }
}
