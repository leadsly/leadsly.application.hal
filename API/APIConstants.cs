namespace API
{
    public class APIConstants
    {
        public class TokenOptions
        {
            public const string ExpiredToken = "token-expired";
        }

        public class RefreshToken
        {
            public const string RefreshTokenProvider = "refresh-token-provider";
            public const string Purpose_RememberMe = "remember-me";
            public const string RememberMe_RefreshToken = "refresh-token";
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
