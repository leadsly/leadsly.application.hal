using Leadsly.Application.Model;

namespace Domain.Models.Responses
{
    public class SignInResultResponse
    {
        public bool InvalidCredentials { get; set; }
        public bool TwoFactorAuthRequired { get; set; }
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
