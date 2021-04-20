using Domain.Models;

namespace API.Authentication
{
    public class RenewAccessTokenResultModel
    {
        public bool Succeeded { get; set; } = false;
        public ApplicationAccessTokenModel AccessToken { get; set; }
    }
}
