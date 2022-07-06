namespace Domain.Models.Responses
{
    public class TwoFactorAuthResultResponse
    {
        public bool FailedToEnterCode { get; set; }
        public bool InvalidOrExpiredCode { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
