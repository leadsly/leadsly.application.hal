using Leadsly.Models.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataCore.Configurations
{
    public static class IdentityUsersConfiguration
    {
        public static void ConfigureIdentityUsersTableNames(ModelBuilder builder, ILogger<DatabaseContext> logger)
        {
            logger.LogInformation("Setting ApplicationUser database table name to 'Users'.");
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users");
            });

            logger.LogInformation("Setting IdentityRole database table name to 'Roles'.");
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            logger.LogInformation("Setting IdentityUserRole database table name to 'UserRoles'.");
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
                //in case you chagned the TKey type
                //  entity.HasKey(key => new { key.UserId, key.RoleId });
            });

            logger.LogInformation("Setting IdentityUserClaim database table name to 'UserClaims'.");
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            logger.LogInformation("Setting IdentityUserLogin database table name to 'UserLogins'.");
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
                //in case you chagned the TKey type
                //  entity.HasKey(key => new { key.ProviderKey, key.LoginProvider });       
            });

            logger.LogInformation("Setting IdentityRoleClaim database table name to 'RoleClaims'.");
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            logger.LogInformation("Setting IdentityUserToken database table name to 'UserTokens'.");
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
                //in case you chagned the TKey type
                // entity.HasKey(key => new { key.UserId, key.LoginProvider, key.Name });

            });
        }
    }
}
