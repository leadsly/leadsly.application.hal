namespace Domain
{
    public enum AfterSignInResult
    {
        None,
        TwoFactorAuthRequired,
        HomePage,
        InvalidCredentials,
        Unknown
    }
}
