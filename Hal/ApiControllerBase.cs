using Hal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Shared.Api
{
    /// <summary>
    /// Base class the API controllers.
    /// </summary>
    public class ApiControllerBase : Controller
    {

        /// <summary>
        /// Produces api response when request fails to successfully complete.
        /// </summary>
        /// <param name="problemDetails"></param>
        /// <returns></returns>
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
        /// Bad request when there is an issue creating a new user.
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
        /// Bad request when server fails to generate token. This can be either change email token, or change password token, or email confirmation token etc.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToGenerateToken()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToGenerateToken,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request failed to send out email.
        /// </summary>        
        /// <returns></returns>
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
        /// Bad request when server fails send email.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToSendConfirmationEmail()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToSendConfirmationEmail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when server fails send email change email confirmation.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToSendChangeEmailConfirmation()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToSendChangeEmailLink,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when server fails to find token in the request. This can be either change email token, or change password token, or email confirmation token etc.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_TokenNotFound()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TokenNotFound,
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
        protected ObjectResult BadRequest_SetupVerificationCodeIsInvalid()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TwoFactorAuthSetupVerificationCode,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }

        /// <summary>
        /// Bad request when users password fails to update.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToUpdatePassword()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToUpdatePassword,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when users email fails to update.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToUpdateEmail()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToUpdateEmail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when there is an issue confirming user's email.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_FailedToConfirmUsersEmail()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToConfirmUsersEmail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when there is an issue updating user's password.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_EmailNotUpdated(IEnumerable<IdentityError> errors)
        {
            Dictionary<string, string[]> errorsDictionary = errors.ToDictionary(x => x.Code, x => new[] { x.Description });

            return ProblemDetailsResult(new ValidationProblemDetails(errorsDictionary)
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToUpdateEmail,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when there is an issue updating user's password.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_PasswordNotUpdated(IEnumerable<IdentityError> errors)
        {
            Dictionary<string, string[]> errorsDictionary = errors.ToDictionary(x => x.Code, x => new[] { x.Description });

            return ProblemDetailsResult(new ValidationProblemDetails(errorsDictionary)
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToUpdatePassword,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request when two step verification code is invalid.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_TwoStepVerificationCodeOrProviderIsInvalid()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TwoStepVerificationCodeOrProviderIsInvalid,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }

        /// <summary>
        /// Bad request when recovery code is not valid.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_RecoveryCodeIsNotValid()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.TwoFactorAuthRecoveryCode,
                Instance = this.HttpContext.Request.Path.Value
            }); ;
        }

        /// <summary>
        /// Bad request when an error occurs while disabling two factor authentication.
        /// </summary>
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

        /// <summary>
        /// Bad request when an error occurs while enabling two factor authentication.
        /// </summary>
        /// <returns></returns>
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
        /// Bad request when user registration error occurs.
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

        /// <summary>
        /// Bad request when temporariy password value is null.
        /// </summary>        
        /// <returns></returns>
        protected ObjectResult BadRequest_NoTempPasswordValue()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequestType,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.NoTempPasswordValue,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Unauthorized request when user credentials are invalid.
        /// </summary>
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
        /// Unauthorized request when refresh token fails.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult Unauthorized_AccessTokenRefreshFailed()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedRefreshTokenFailed,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// Bad request user sent correct username/email but invalid password.
        /// </summary>
        /// <param name="failedAttempts"></param>
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

        /// <summary>
        /// Bad request when user exceeded maximum number of sign in attempts and the account has now been locked.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Forbidden when user's email has not been confirmed.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult Forbidden_EmailNotConfirmed()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Forbidden,
                Status = StatusCodes.Status403Forbidden,
                Title = ReasonPhrases.GetReasonPhrase(404),
                Detail = ProblemDetailsDescriptions.ForbiddenEmailNotConfirmed,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When external provider token (Google, Facebook etc) is invalid.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// When user registration token is invalid.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult Unauthorized_InvalidRegistrationToken()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.Unauthorized,
                Status = StatusCodes.Status401Unauthorized,
                Title = ReasonPhrases.GetReasonPhrase(401),
                Detail = ProblemDetailsDescriptions.UnauthorizedRegistrationToken,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When cannot retrieve stripe order by id.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult BadRequest_StripeCustomerDoesNotExist()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.StripeCustomerDoesNotExist,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When stripe customer does not have email address configured.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult BadRequest_NoEmailStripe()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.NoEmailFound_Stripe,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When server failed to extract stripe webhook event type.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult BadRequest_EventExtractionFailed_Stripe()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.NoEmailFound_Stripe,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When server failed to deserialize object.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult BadRequest_DeserializationFailed()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.FailedToDeserialize,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

        /// <summary>
        /// When no campaign is found witht he given id.
        /// </summary>
        /// <returns></returns>
        protected ObjectResult BadRequest_CampaignNotFound()
        {
            return ProblemDetailsResult(new ProblemDetails
            {
                Type = ProblemDetailsTypes.BadRequest,
                Status = StatusCodes.Status400BadRequest,
                Title = ReasonPhrases.GetReasonPhrase(400),
                Detail = ProblemDetailsDescriptions.CampaignNotFound,
                Instance = this.HttpContext.Request.Path.Value
            });
        }

    }
}
