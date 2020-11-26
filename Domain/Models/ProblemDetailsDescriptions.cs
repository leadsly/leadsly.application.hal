using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public static class ProblemDetailsDescriptions
    {
        /// <summary>Bad Request Detail</summary>
        public const string BadRequestDetail = "Some of the provided input data is invalid";
        public const string BadRequest = "Some of the provided input data is invalid";

        /// <summary>Forbidden Detail</summary>
        public const string ForbiddenDetail = "The server understood the request, but is refusing to authorize it";

        public const string Forbidden = "The server understood the request, but is refusing to authorize it";

        /// <summary>Internal Server Error Detail</summary>
        public const string InternalServerErrorDetail = "An unexpected error has occurred";

        public const string InternalServerError = "An unexpected error has occurred";

        /// <summary>Method Not Allowed Detail</summary>
        public const string MethodNotAllowedDetail = "The request method is not supported by the target resource";
        public const string MethodNotAllowed = "The request method is not supported by the target resource";

        /// <summary>Not Acceptable Detail</summary>
        public const string NotAcceptableDetail = "The target resource cannot be returned in the media type requested";

        public const string NotAcceptable = "The target resource cannot be returned in the media type requested";

        /// <summary>Not Found Detail</summary>
        public const string NotFoundDetail = "The requested resource cannot be found";
        public const string NotFound = "The requested resource cannot be found";

        /// <summary>Unauthorized Detail</summary>
        public const string UnauthorizedDetail = "Authentication credentials are missing or invalid";
        public const string Unauthorized = "Authentication credentials are missing or invalid";
    }
}
