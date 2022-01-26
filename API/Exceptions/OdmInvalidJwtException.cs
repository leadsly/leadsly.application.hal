using Domain;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace API.Exceptions
{
    public class BotLeadslyInvalidJwtException : InvalidJwtException, IBotLeadslyWebApiException
    {
        public BotLeadslyInvalidJwtException(string message) : base(message) { }

        public string Type => ProblemDetailsTypes.InternalServerErrorType;

        public string Title => ReasonPhrases.GetReasonPhrase(500);

        public int Status => StatusCodes.Status500InternalServerError;

        public string Detail => ProblemDetailsDescriptions.ExternalJwtIsInvalid;

        public string Instance => "/api/auth/external-signin";
    }
}
