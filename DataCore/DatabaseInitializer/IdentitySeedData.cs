using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace DataCore.DatabaseInitializer
{
    public class IdentitySeedData
    {
        public async static Task Populate(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            UserManager<ApplicationUser> userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();

            await Seed(userManager, roleManager, configuration, logger);
        }

        private async static Task Seed(UserManager<ApplicationUser> userManager, 
                                       RoleManager<IdentityRole> roleManager, 
                                       IConfiguration configuration, 
                                       ILogger<DatabaseInitializer> logger)
        {
            string[] roles = new string[] { "admin", "user" };

            foreach (string role in roles)
            {
                if (!roleManager.Roles.Any(r => r.Name == role))
                {
                    logger.LogInformation($"No roles found matching '{ role }' role.");

                    IdentityRole newRole = new IdentityRole
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    };
                    
                    try
                    {
                        logger.LogInformation($"Creating '{ role }' role.");
                        await roleManager.CreateAsync(newRole);                        
                    }
                    catch(Exception ex)
                    {
                        logger.LogError(ex, $"Error occured when creating { role } role.");
                        throw;
                    }

                    if (role == "admin" || role == "user")
                    {
                        try
                        {
                            await roleManager.AddClaimAsync(newRole, new Claim("permission", "create"));
                            logger.LogInformation($"Successfully added { role } role claim 'permission': 'create'");

                            await roleManager.AddClaimAsync(newRole, new Claim("permission", "update"));
                            logger.LogInformation($"Successfully added  { role } role claim 'permission': 'update'");

                            await roleManager.AddClaimAsync(newRole, new Claim("permission", "retrieve"));
                            logger.LogInformation($"Successfully added  { role } role claim 'permission': 'retrieve'");

                            await roleManager.AddClaimAsync(newRole, new Claim("permission", "delete"));
                            logger.LogInformation($"Successfully added  { role } role claim 'permission': 'delete'");
                        }
                        catch(Exception ex)
                        {
                            logger.LogError(ex, $"Error occured when adding 'permission' claims.");
                            throw;
                        }
                    }
                }
            }

            ApplicationUser admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@admin.com",
                FirstName = "Kai",
                LastName = "Kami",
                PhoneNumber = "800-000-0000"
            };

            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

            if (!userManager.Users.Any(u => u.UserName == admin.UserName))
            {
                logger.LogInformation($"No users found matching { admin.UserName } username.");

                try
                {
                    logger.LogInformation($"Hashing { admin.UserName } password.");
                    string hashed = passwordHasher.HashPassword(admin, configuration["Admin:Password"]);
                    logger.LogInformation($"Successfully hashed { admin.UserName } password.");
                    admin.PasswordHash = hashed;                    
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"Error occured when hashing { admin.UserName } password or when retrieving password from 'configuration[Admin:Password]'");
                    throw;
                }

                try
                {
                    await userManager.CreateAsync(admin);
                    logger.LogInformation($"Successfully created { admin.UserName } user.");

                    await userManager.AddToRoleAsync(admin, "admin");
                    logger.LogInformation($"Successfully added { admin.UserName } to 'admin' role.");
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"Error occured when when creating user or when adding user to role");
                    throw;
                }
            }
        }
    }
}
