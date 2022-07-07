namespace Domain
{
    public enum TwoFactorAuthResult
    {
        None,
        InvalidOrExpiredCode,
        SignedIn,
        ToastErrorMessage,
        UnexpectedError,
        Unknown
    }
}
