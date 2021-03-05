using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace API.DataProtectorTokenProviders
{
    public class StaySignedInDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public StaySignedInDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<StaySignedInDataProtectionTokenProviderOptions> options, ILogger<StaySignedInDataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        private readonly StaySignedInDataProtectionTokenProviderOptions _options;
        private readonly ILogger<DataProtectorTokenProvider<TUser>> _logger;
               
    }
}
