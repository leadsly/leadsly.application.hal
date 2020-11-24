using DataCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Domain.DbInfo;
using Microsoft.AspNetCore.Identity;
using Domain.Models;
using API.Auth;

namespace API.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddConnectionProviders(this IServiceCollection services, IConfiguration configuration)
        {
            string defaultConnection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DatabaseContext>(options =>
            {                
                options.UseSqlServer(defaultConnection);
            }, ServiceLifetime.Scoped);

            services.AddSingleton(new DbInfo(defaultConnection));

            return services;
        }

        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
        {
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromDays(7);
            });

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddDefaultTokenProviders()
            .AddTokenProvider("DataProtector", typeof(DataProtectorTokenProvider<ApplicationUser>))
            .AddRoles<IdentityRole>()
            .AddUserManager<UserManager<ApplicationUser>>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<DatabaseContext>(); // Tell identity which EF DbContext to use;

            //Configure Claims Identity
            services.AddScoped<IGetIdentity, GetIdentity>();

            return services;

        }
    }
}
