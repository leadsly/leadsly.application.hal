using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;

namespace API.Controllers
{
    public class ApiControllerBase : Controller
    {
        protected ObjectResult ProblemDetailsResult(ProblemDetails problemDetails)
        {
            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentTypes =
                {
                    new MediaTypeHeaderValue(new Microsoft.Extensions.Primitives.StringSegment("application/problem+json")),
                }
            };
        }

        /// <summary>
        /// Bad request when there is an issue signing up a new user.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_UserNotCreated(IEnumerable<IdentityError> errors)
        {
            Dictionary<string, string[]> errorsDictionary = errors.ToDictionary(x => x.Code, x => new[] { x.Description });

            return ProblemDetailsResult(new ValidationProblemDetails(errorsDictionary)
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.RegistrationDetail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when user is not found.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_UserNotFound()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.UserNotFound,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }

        /// <summary>
        /// Bad request when two factor authentication setup verification code is invalid.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_VerificationCodeIsInvalid()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TwoFactorAuthVerificationCode,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }        

        /// <summary>
        /// Bad request when an error occurs while disabling two factor authentication for a user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToDisable2fa()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToDisable2fa,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        protected ObjectResult BadRequest_FailedToEnable2fa()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToEnable2fa,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when an error occurs while resetting authenticator key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToResetAuthenticatorKey()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToResetAuthenticatorKey,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when an error occurs while resetting authenticator key.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult BadRequest_TwoFactorAuthenticationIsNotEnabled()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TwoFactorAuthenticationIsNotEnabled,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when request attempts to disable two factor authentication when it is not enabled.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected ObjectResult BadRequest_CannotDisable2faWhenItsNotEnabled()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.CannotDisable2faWhenItsNotEnabled,
                Instance = this.HttpContext.Request.Path.Value
            });
        }


        /// <summary>
        /// Bad request when user is not found.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_UserRegistrationError()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.RegistrationErrorDetail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        protected ObjectResult BadRequest_FailedToSendEmail()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToSendEmail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when cannot find user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        protected ObjectResult Unauthorized_InvalidCredentials()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedDetail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when cannot find user by email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        protected ObjectResult Unauthorized_InvalidCredentials(int failedAttempts)
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = $"{ ProblemDetailsDescriptions.UnauthorizedDetail } Failed attempt: {failedAttempts}.",
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        protected ObjectResult Unauthorized_AccountLockedOut()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedAccountLocked,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        protected ObjectResult Unauthorized_InvalidExternalProviderToken()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedExternalProvider,
                Instance = this.HttpContext.Request.Path.Value
            });
        }
    }
}
