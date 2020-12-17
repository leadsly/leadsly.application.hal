using API.Authentication;
using API.Extensions;
using Domain.Models;
using Domain.Supervisor;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class AuthController : APIControllerBase
    {
        public AuthController(            
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,            
            ILogger<AuthController> logger)
        {            
            _tokenService = tokenService;
            _claimsIdentityService = claimsIdentityService;            
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }
        
        private readonly IAccessTokenService _tokenService;
        private readonly IClaimsIdentityService _claimsIdentityService;        
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;

        [HttpPost]
        [AllowAnonymous]
        [Route("external-signin")]
        public async Task<IActionResult> ExternalSiginOrSignup([FromBody] SocialUserModel externalUser)
        {
            this._logger.LogDebug("ExternalSigninOrSignup action executed.");

            ApplicationUser appUser = await _userManager.FindByNameAsync(externalUser.Email);

            // create user
            if(appUser == null)
            {
                appUser = new ApplicationUser
                {

                }   
            }
        }

            [HttpPost]
        [AllowAnonymous]
        [Route("signin")]
        public async Task<IActionResult> SignIn([FromBody] SigninUserModel signin, CancellationToken ct = default)
        {
            this._logger.LogDebug("SignIn action executed.");

            if (string.IsNullOrEmpty(signin.Password))
            {
                return Unauthorized_InvalidCredentials();
            }

            ApplicationUser appUser = await _userManager.FindByEmailAsync(signin.Email);

            if (appUser == null)
            {
                return Unauthorized_InvalidCredentials();
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            if (await _userManager.CheckPasswordAsync(appUser, signin.Password) == false)
            {
                return Unauthorized_InvalidCredentials();
            }

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser, _userManager, _roleManager);

            ApplicationAccessToken accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await _userManager.RemoveAuthenticationTokenAsync(appUser, APIConstants.RefreshToken.RefreshTokenProvider, APIConstants.RefreshToken.RememberMe_RefreshToken);

            if(signin.RememberMe == true)
            {
                string refreshToken = await _userManager.GenerateUserTokenAsync(appUser, APIConstants.RefreshToken.RefreshTokenProvider, APIConstants.RefreshToken.Purpose_RememberMe);

                await _userManager.SetAuthenticationTokenAsync(appUser, APIConstants.RefreshToken.RefreshTokenProvider, APIConstants.RefreshToken.RememberMe_RefreshToken, refreshToken);                
            }            

            return Ok(accessToken);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignupUserModel signupModel, CancellationToken ct = default)
        {
            _logger.LogDebug("Signup action executed.");

            ApplicationUser newUser = new ApplicationUser
            {
                Email = signupModel.Email,
                UserName = signupModel.Email
            };

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            IdentityResult result = await _userManager.CreateAsync(newUser, signupModel.Password);

            if (result.Succeeded == false)
            {
                _logger.LogError("User registration data is invalid or missing.");

                return BadRequest_UserNotCreated(result.Errors);
            }

            ApplicationUser signedupUser = await _userManager.FindByEmailAsync(signupModel.Email);

            if (signedupUser == null)
            {
                _logger.LogError($"Unable to find user by email: { signupModel.Email }. Attempted to retrieve newly registered user to generate access token.");

                return BadRequest_UserRegistrationError();
            }

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(signedupUser, _userManager, _roleManager);

            ApplicationAccessToken accessToken = await _tokenService.GenerateApplicationTokenAsync(signedupUser.Id, claimsIdentity);

            return Ok(accessToken);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            _logger.LogDebug("CheckIfEmailExists action executed. Looking for: {email}", email);

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            return user == null ? new JsonResult(false) : new JsonResult(true);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            _logger.LogDebug("RefreshToken action executed.");

            RenewAccessTokenResult result = new RenewAccessTokenResult();

            string expiredAccessToken = HttpContext.GetAccessToken();

            if(expiredAccessToken != string.Empty)
            {
                // request has token but it failed authentication. Attempt to renew the token
                result = await _tokenService.TryRenewAccessToken(expiredAccessToken, _userManager, _roleManager);
                bool succeeded = result.Succeeded;
                _logger.LogDebug("Attempted to rewnew jwt. Result: {succeeded}", succeeded);
            }

            return Ok(result);
        }        

    }
}
