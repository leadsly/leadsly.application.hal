using API.Authentication;
using API.Extensions;
using Domain.Models;
using Domain.Supervisor;
using Domain.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            ISupervisor supervisor,
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<AuthController> logger)
        {
            _supervisor = supervisor;
            _tokenService = tokenService;
            _claimsIdentityService = claimsIdentityService;
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly IAccessTokenService _tokenService;
        private readonly IClaimsIdentityService _claimsIdentityService;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;

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

        [HttpGet]
        [Route("refresh-token")]
        public IActionResult RefreshToken()
        {
            _logger.LogDebug("RefreshToken action executed.");

            // JwtRefreshToken middleware will catch this request, and if the request contains jwt that is valid, then it will automatically renew jwt
            string jwt = HttpContext.GetAccessToken();

            ApplicationAccessToken accessToken = new ApplicationAccessToken
            {
                access_token = jwt,
                expires_in = (long)_jwtOptions.ValidFor.TotalSeconds
            };

            return Ok(accessToken);
        }

    }
}
