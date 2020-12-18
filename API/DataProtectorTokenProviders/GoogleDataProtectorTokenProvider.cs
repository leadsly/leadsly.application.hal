using Google.Apis.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.DataProtectorTokenProviders
{
    public class GoogleDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public GoogleDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<GoogleDataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        private readonly GoogleDataProtectionTokenProviderOptions _options;
        private readonly ILogger<DataProtectorTokenProvider<TUser>> _logger;

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            bool valid = false;
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string>() { _options.ClientId }
                };

                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                valid = true;
            }
            catch(InvalidJwtException ex)
            {
                _logger.LogError("Google token is invalid or has been tampered with.", ex);
            }

            return valid;
            
        }

    }
}
