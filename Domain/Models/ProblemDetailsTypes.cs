namespace Domain.Models
{
    public static class ProblemDetailsTypes
    {
        /// <summary>Bad Request Type </summary>
        public const string BadRequestType = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        public const string BadRequest = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        /// <summary>Internal Server Error Type </summary>
        public const string InternalServerErrorType = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        /// <summary>Unauthorized Type </summary>
        public const string Unauthorized = "https://tools.ietf.org/html/rfc7235#section-3.1";

        /// <summary>Forbidden Type </summary>
        public const string Forbidden = "https://tools.ietf.org/html/rfc7231#section-6.5.3";

        /// <summary>Not Found Type </summary>
        public const string NotFound = "https://tools.ietf.org/html/rfc7231#section-6.5.4";

        /// <summary>Method Not Allowed Type </summary>
        public const string MethodNotAllowedType = "https://tools.ietf.org/html/rfc7231#section-6.5.5";
        public const string MethodNotAllowed = "https://tools.ietf.org/html/rfc7231#section-6.5.5";

        /// <summary>Not Acceptable Type </summary>
        public const string NotAcceptable = "https://tools.ietf.org/html/rfc7231#section-6.5.6";

        /// <summary>Conflict Type </summary>
        public const string Conflict = "https://tools.ietf.org/html/rfc7231#section-6.5.8";

        /// <summary>Unsupported Media Type Type </summary>
        public const string UnsupportedMediaType = "https://tools.ietf.org/html/rfc7231#section-6.5.13";

        /// <summary>Unprocessible Entity Type </summary>
        public const string UnprocessibleEntity = "https://tools.ietf.org/html/rfc4918#section-11.2";

        /// <summary>Internal Server Error Type </summary>
        public const string InternalServerError = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    }
}
