using Microsoft.AspNetCore.Identity;

namespace API.DataProtectorTokenProviders
{
    public class FacebookDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
