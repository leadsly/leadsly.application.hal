using Domain;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    /// <summary>
    /// Base class the API controllers.
    /// </summary>
    public class ApiControllerBase : Controller
    {
        /// <summary>
        /// Sets or refreshes user's refresh token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appUser"></param>
        /// <param name="_userManager"></param>
        /// <param name="_logger"></param>
        /// <returns></returns>
        public async Task SetOrRefreshStaySignedInToken<T>(ApplicationUser appUser, OdmUserManager _userManager, ILogger<T> _logger)
        {
            string tokenName = ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName;
            _logger.LogDebug("Attempting to remove user's [{tokenName}] token.", tokenName);
            IdentityResult removalResult = await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, tokenName);
            bool removeTokenResult = removalResult.Succeeded;
            _logger.LogDebug("Was [{tokenName}] token removal operation was successful: {removeTokenResult}", tokenName, removeTokenResult);

            string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose);
            bool successfullyGenerated = refreshToken != null;
            _logger.LogDebug("Was user's token was successfully generated: {successfullyGenerated}", successfullyGenerated);

            _logger.LogDebug("Setting user's new [{tokenName}] token.", tokenName);
            IdentityResult setResult = await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, tokenName, refreshToken);
            bool setTokenResult = setResult.Succeeded;
            _logger.LogDebug("Was [{tokenName}] token successfully set: {setTokenResult}.", tokenName, setTokenResult);

        }

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
            }); ;
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
        /// Bad request when an issue occured sending an email.
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
        /// Bad request user credentials are invalid.
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
    }
}
