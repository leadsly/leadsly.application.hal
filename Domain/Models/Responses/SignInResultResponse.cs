using Leadsly.Application.Model;

namespace Domain.Models.Responses
{
    public class SignInResultResponse
    {
        public bool InvalidPassword { get; set; } = false;
        public bool InvalidEmail { get; set; } = false;
        public bool TwoFactorAuthRequired { get; set; }
        public bool EmailPinChallenge { get; set; } = false;
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
