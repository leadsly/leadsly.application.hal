namespace Domain
{
    public class ApiConstants
    {
        public class TwoFactorAuthentication
        {
            public const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        }

        public class TokenOptions
        {
            public const string ExpiredToken = "token-expired";
        }

        public class AspNetUserTokens
        {
            public const string AspNetUserStore_LoginProvider = "[AspNetUserStore]";
            public const string Name = "RecoveryCodes";            
        }

        public class DataTokenProviders
        {
            public class RefreshTokenProvider
            {
                public const string Name = "refresh-token-provider";
                public const string Purpose = "remember-me";
                public const string RememberMe = "refresh-token";
            }

            public class ExternalLoginProviders
            {
                public const string Google = "GOOGLE";
                public const string Facebook = "FACEBOOK";
                public const string IdToken = "id_token";
                public const string AuthToken = "auth_token";
            }
        }

        public class Email
        {
            public const string CallbackUrlToken = "CallbackUrlToken";
        }

        public class VaultKeys
        {
            public const string JwtSecret = "Jwt:Secret";
            public const string GoogleClientId = "Google:ClientId";
            public const string GoogleClientSecret = "Google:ClientSecret";
            public const string FaceBookClientId = "Facebook:ClientId";
            public const string FaceBookClientSecret = "Facebook:ClientSecret";
            public const string AdminPassword = "Admin:Password";
            public const string SystemAdminEmailPassword = "System:Admin:Email:Password";
            public const string SystemAdminEmail = "System:Admin:Email";
            public const string TwoFactorAuthenticationEncryptionKey = "TwoFactorAuthentication:EncryptionKey";
        }

        public class Cors
        {
            public const string WithOrigins = "WithClientOrigins";
            public const string AllowAll = "AllowAll";
        }        

        public class Jwt
        {
            public const string DefaultAuthorizationPolicy = "Bearer";

            public class ClaimIdentifiers
            {
                public const string Role = "role";
                public const string UserName = "username";
                public const string Permission = "permission";
            }

        }
    }
}
