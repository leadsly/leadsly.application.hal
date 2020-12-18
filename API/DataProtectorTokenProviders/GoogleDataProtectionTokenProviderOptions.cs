using Microsoft.AspNetCore.Identity;

namespace API.DataProtectorTokenProviders
{
    public class GoogleDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
