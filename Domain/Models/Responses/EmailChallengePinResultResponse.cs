namespace Domain.Models.Responses
{
    public class EmailChallengePinResultResponse
    {
        public bool TwoFactorAuthRequired { get; set; }
        public bool FailedToEnterPin { get; set; }
        public bool InvalidOrExpiredPin { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
