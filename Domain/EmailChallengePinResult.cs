namespace Domain
{
    public enum EmailChallengePinResult
    {
        None,
        TwoFactorAuthRequired,
        InvalidOrExpiredPin,
        SignedIn,
        ToastErrorMessage,
        UnexpectedError,
        Unknown
    }
}
