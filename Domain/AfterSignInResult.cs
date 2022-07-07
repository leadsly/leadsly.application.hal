namespace Domain
{
    public enum AfterSignInResult
    {
        None,
        TwoFactorAuthRequired,
        HomePage,
        InvalidEmail,
        InvalidPassword,
        ToastMessageError,
        Unknown
    }
}
