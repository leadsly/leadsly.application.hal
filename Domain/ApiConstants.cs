namespace Domain
{
    public class ApiConstants
    {
        public class TwoFactorAuthentication
        {
            public const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            public const int NumberOfRecoveryCodes = 5;
        }

        public class TokenOptions
        {
            public const string ExpiredToken = "token-expired";
        }

        public class DataTokenProviders
        {
            public class AspNetUserProvider
            {
                public const string ProviderName = "[AspNetUserStore]";
                public const string TokenName = "RecoveryCodes";
            }

            public class StaySignedInProvider
            {
                public const string ProviderName = "[UserSession]";
                public const string Purpose = "Keep user signed in.";
                public const string TokenName = "StaySignedIn";
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
            public const string ChangeEmailUrl = "{clientAddress}/users/{id}/change-email?oldEmail={oldEmail}&newEmail={newEmail}&code={code}";
            public const string ClientAddress = "{ClientAddress}";
            public const string IdParam = "{Id}";
            public const string EmailParam = "{Email}";
            public const string TokenParam = "{Token}";

            public class Change            
            {
                public const string Url = "{ClientAddress}/auth/{Id}/email-change-confirmation?newEmail={Email}&token={Token}";
            }

            public class Verify
            {
                public const string Url = "{ClientAddress}/auth/email-confirmation?email={Email}&token={Token}";                
            }
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
