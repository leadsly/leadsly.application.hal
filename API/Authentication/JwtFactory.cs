using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Authentication.Jwt
{
    public class JwtFactory : IJwtFactory
    {   
        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, ILogger<IJwtFactory> logger)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
            _logger = logger;
        }

        private readonly JwtIssuerOptions _jwtOptions;
        private readonly ILogger<IJwtFactory> _logger;

        public async Task<string> GenerateEncodedJwtAsync(string userId, ClaimsIdentity identity)
        {
            _logger.LogInformation("Generating jwt for user id: {userId}", userId);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Sub, userId)
            };

            claims.AddRange(identity.Claims);

            // Create the JWT security token and encode it.
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience, // Audience cannot be null
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private static long ToUnixEpochDate(DateTime date)
        {
            DateTime universalTime = date.ToUniversalTime();
            DateTimeOffset offset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            double totalSeconds = (universalTime - offset).TotalSeconds;            

            return (long)Math.Round(totalSeconds);
        }

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }

    }
}
