namespace Domain
{
    public enum AfterSignInResult
    {
        None,
        TwoFactorAuthRequired,
        EmailPinChallenge,
        HomePage,
        InvalidEmail,
        InvalidPassword,
        ToastMessageError,
        Unknown
    }
}
