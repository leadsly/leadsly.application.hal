namespace Hal
{
    public static class ProblemDetailsDescriptions
    {
        public const string ExpiredAccessTokenIsInvalid = "Expired access token is invalid.";
        public const string BadRequest = "Some of the provided input data is invalid.";
        public const string ForbiddenDetail = "The server understood the request, but is refusing to authorize it.";
        public const string ExternalJwtIsInvalid = "External provider jwt is invalid.";
        public const string Unauthorized = "Authentication credentials are missing or invalid.";
        public const string LeadslySocialAccountAuthenticationError = "Failed to authenticate user's social account";
    }
}
