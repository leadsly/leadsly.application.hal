namespace API
{
    public class APIConstants
    {
        public class TokenOptions
        {
            public const string ExpiredToken = "Token-Expired";
        }

        public class Cors
        {
            public const string WithOrigins = "WithClientOrigins";
            public const string AllowAll = "AllowAll";
        }

        public class JwtClaimIdentifiers
        {
            public const string Role = "role";
            public const string Id = "id";
            public const string UserName = "username";
            public const string Permission = "permission";
        }
    }
}
