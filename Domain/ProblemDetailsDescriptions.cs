namespace Domain
{
    public static class ProblemDetailsDescriptions
    {
        public const string BadRequestDetail = "Some of the provided input data is invalid.";
        public const string BadRequest = "Some of the provided input data is invalid.";
        public const string ForbiddenDetail = "The server understood the request, but is refusing to authorize it.";
        public const string Forbidden = "The server understood the request, but is refusing to authorize it.";
        public const string InternalServerErrorDetail = "An unexpected error has occurred.";
        public const string InternalServerError = "An unexpected error has occurred.";
        public const string MethodNotAllowedDetail = "The request method is not supported by the target resource.";
        public const string MethodNotAllowed = "The request method is not supported by the target resource.";
        public const string NotAcceptableDetail = "The target resource cannot be returned in the media type requested.";
        public const string NotAcceptable = "The target resource cannot be returned in the media type requested.";
        public const string NotFoundDetail = "The requested resource cannot be found.";
        public const string NotFound = "The requested resource cannot be found.";
        public const string UnauthorizedDetail = "Authentication credentials are invalid.";
        public const string UnauthorizedAccountLocked = "Account is locked.";
        public const string UnauthorizedExternalProvider = "Invalid external provider token";
        public const string Unauthorized = "Authentication credentials are missing or invalid.";
        public const string ExpiredAccessTokenIsInvalid = "Expired access token is invalid.";
        public const string ExternalJwtIsInvalid = "External provider jwt is invalid.";
        public const string RegistrationErrorDetail = "User registration error occured.";        
        public const string RegistrationDetail = "Missing or invalid registration data.";
        public const string UserNotFound = "User could not be found.";
        public const string TwoFactorAuthSetupVerificationCode = "Two factor authentication code is invalid.";
        public const string TwoStepVerificationCodeOrProviderIsInvalid = "Verification code or provider is invalid.";
        public const string TwoFactorAuthRecoveryCode = "Recovery code is invalid.";
        public const string CannotDisable2faWhenItsNotEnabled = "Cannot disable 2fa as it's not currently enabled.";
        public const string TwoFactorAuthenticationIsNotEnabled = "Two factor authentication is not enabled.";
        public const string FailedToDisable2fa = "An error occured while disabling 2fa.";
        public const string FailedToEnable2fa = "An error occured while enabling 2fa.";
        public const string FailedToResetAuthenticatorKey = "An error occured while resetting 2fa authenticator key.";
        public const string FailedToSendEmail = "Failed to send email.";
        public const string FailedToUpdatePassword = "Failed to update password.";
        public const string FailedToUpdateEmail = "Failed to update e-mail.";
        public const string FailedToVerifyUsersEmail = "Failed to verify user's e-mail.";
        public const string FailedToGenerateChangeEmailToken = "Failed to generate change e-mail token.";
        public const string PasswordResetTokenNotFound = "Password reset token was not found in the request.";
    }
}
