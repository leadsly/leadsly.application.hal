using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace Hal.Exceptions
{
    public class LeadslySecurityTokenException : SecurityTokenException, ILeadslyWebApiException
    {
        public string Type => ProblemDetailsTypes.InternalServerErrorType;

        public string Title => ReasonPhrases.GetReasonPhrase(500);

        public int Status => StatusCodes.Status500InternalServerError;

        public string Detail => ProblemDetailsDescriptions.ExpiredAccessTokenIsInvalid;

        public string Instance => "/api/auth/refresh-token";
    }
}
