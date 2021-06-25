using DataCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Domain.DbInfo;
using Microsoft.AspNetCore.Identity;
using Domain.Models;
using API.Authentication;
using API.Authentication.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Domain.Supervisor;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Domain;
using System.Linq;
using Serilog;
using API.DataProtectorTokenProviders;
using API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace API.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddSupervisorConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering supervisor services.");

            services.AddScoped<ISupervisor, Supervisor>();

            return services;
        }

        public static IServiceCollection AddConnectionProviders(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            Log.Information("Configuring default connection string and database context.");

            string defaultConnection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DatabaseContext>(options =>
            {                
                options.UseSqlServer(defaultConnection);
                options.EnableSensitiveDataLogging(env.IsDevelopment());
            }, ServiceLifetime.Scoped);
            
            services.AddSingleton(new DbInfo(defaultConnection));

            return services;
        }

        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Adding identity services.");

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddDefaultTokenProviders()
            .AddTokenProvider<StaySignedInDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName)
            .AddTokenProvider<FacebookDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook)
            .AddTokenProvider<GoogleDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.ExternalLoginProviders.Google)
            .AddTokenProvider<EmailConfirmationDataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider)
            .AddRoles<IdentityRole>()            
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddSignInManager()
            .AddUserManager<OdmUserManager>()
            .AddEntityFrameworkStores<DatabaseContext>(); // Tell identity which EF DbContext to use;

            // email token provider settings
            services.Configure<EmailConfirmationDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = TokenOptions.DefaultEmailProvider;
                options.TokenLifespan = TimeSpan.FromDays(3);
            });

            // stay signed in provider settings
            services.Configure<StaySignedInDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName;
                options.TokenLifespan = TimeSpan.FromDays(7);
            });

            // facebook token provider settings
            services.Configure<FacebookDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook;
                options.TokenLifespan = TimeSpan.FromDays(120);
                options.ClientId = configuration[ApiConstants.VaultKeys.FaceBookClientId];
                options.ClientSecret = configuration[ApiConstants.VaultKeys.FaceBookClientSecret];
            });

            // google token provider settings
            services.Configure<GoogleDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.ExternalLoginProviders.Google;
                options.TokenLifespan = TimeSpan.FromDays(120);
                options.ClientId = configuration[ApiConstants.VaultKeys.GoogleClientId];
                options.ClientSecret = configuration[ApiConstants.VaultKeys.GoogleClientSecret];
            });

            //Configure Claims Identity
            services.AddScoped<IClaimsIdentityService, ClaimsIdentityService>();

            return services;
        }

        public static IServiceCollection AddJsonWebTokenConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Jwt services.");

            services.AddSingleton<IJwtFactory, JwtFactory>();

            IConfigurationSection jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // retrieve private key from user secrets or azure vault
            string privateKey = configuration[ApiConstants.VaultKeys.JwtSecret];
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(privateKey));
            
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);                
            });

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,                 

                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;                     
                // In case of having an expired token
                configureOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add(ApiConstants.TokenOptions.ExpiredToken, "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<IAccessTokenService, AccessTokenService>();

            return services;
        }

        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring authorization options.");

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                                .AddRequirements(new DenyAnonymousAuthorizationRequirement())
                                .Build();
            });

            return services;
        }

        public static IServiceCollection AddRemoveNull204FormatterConfigration(this IServiceCollection services)
        {
            Log.Information("Configuring output formatters.");

            services.AddControllers(opt =>
            {
                // remove formatter that turns nulls into 204 - Angular http client treats 204s as failed requests
                HttpNoContentOutputFormatter noContentFormatter = opt.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if(noContentFormatter != null)
                {
                    noContentFormatter.TreatNullValueAsNoContent = false;
                }
            });

            return services;
        }

        public static IMvcBuilder AddJsonOptionsConfiguration(this IMvcBuilder builder)
        {
            Log.Information("Configuring NewtonsoftJson options.");

            builder.AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                // Serialize the name of enum values rather than their integer value
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            return builder;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Cors.");

            services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.Cors.WithOrigins, new CorsPolicyBuilder()
                                                      .WithOrigins(configuration["AllowedOrigins"])
                                                      .AllowAnyHeader()
                                                      .AllowCredentials()
                                                      .AllowAnyMethod()
                                                      .Build());


                options.AddPolicy(ApiConstants.Cors.AllowAll, new CorsPolicyBuilder()
                                                     .AllowAnyOrigin()
                                                     .AllowAnyHeader()
                                                     .AllowAnyMethod()
                                                     .Build());
            });

            return services;
        }

        public static IServiceCollection AddApiBehaviorOptionsConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring ApiBehaviorOptions.");

            // Required to surpress automatic problem details returned by asp.net core framework when ModelState.IsValid == false.
            // Allows for custom IActionFilter implementation and response. See InvalidModelStateFilter.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }

        public static IServiceCollection AddEmailServiceConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring AddEmailServiceConfiguration.");

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IHtmlTemplateGenerator, HtmlTemplateGenerator>();

            return services;
        }

    }
}
