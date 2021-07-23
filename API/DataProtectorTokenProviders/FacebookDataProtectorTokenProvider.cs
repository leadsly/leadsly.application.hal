using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.DataProtectorTokenProviders
{
    public class FacebookDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public FacebookDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<FacebookDataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        private readonly FacebookDataProtectionTokenProviderOptions _options;
        private readonly ILogger<DataProtectorTokenProvider<TUser>> _logger;

        public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            HttpResponseMessage response = null;

            using (HttpClient client = new HttpClient())
            {
                // do not get make a request each time. Only when token is expired. This is what is causing the: 
                // sdk.js?hash=b6546f882ac097f1cef0d8faf2cdefb5:49 You are overriding current access token, that means some other app is expecting different access token and you will probably break things. Please consider passing access_token directly to API parameters instead of overriding the global settings. 
                // error in the browser.
                Uri verifyFacebookTokenUrl = new Uri(string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}", token, _options.ClientId, _options.ClientSecret));
                response = await client.GetAsync(verifyFacebookTokenUrl);
            }

            string result = response.IsSuccessStatusCode ? "valid" : "invalid or has been tampered with";

            _logger.LogDebug("Facebook token is {result}.", result);

            return response.IsSuccessStatusCode;
        }
    }
}
