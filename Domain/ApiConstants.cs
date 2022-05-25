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

            public class RegisterNewUserProvider
            {
                public const string ProviderName = "[RegisterUser]";
                public const string Purpose = "Allow only people with registration link to sign up.";
                public const string TokenName = "SignUp";
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
            public const string JwtSecret = "JwtSecret";
            public const string GoogleClientId = "GoogleClientId";
            public const string GoogleClientSecret = "GoogleClientSecret";
            public const string FaceBookClientId = "FacebookClientId";
            public const string FaceBookClientSecret = "FacebookClientSecret";
            public const string AdminPassword = "AdminPassword";
            public const string SystemAdminEmailPassword = "SystemAdminEmailPassword";
            public const string SystemAdminEmail = "System:AdminEmail";
            public const string TwoFactorAuthenticationEncryptionKey = "TwoFactorAuthenticationEncryptionKey";
            public const string StripeSecretKey = "StripeSecreyKey";
            public const string StripeWebhookSecret = "StripeWebhookSecret";
            public const string TempPassword = "TempPassword";
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

        public class PageObjectConstants
        {
            public const string Connect = "Connect";
        }
    }
}
