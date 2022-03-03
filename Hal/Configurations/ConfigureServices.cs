﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
using Hal.OptionsJsonModels;
using Domain.OptionsJsonModels;
using Infrastructure.Repositories;
using Domain.Repositories;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PageObjects.Pages;
using OpenQA.Selenium.Support.UI;
using Domain.Pages;
using Domain.Providers;
using Leadsly.Application.Model;
using Domain.Services;

namespace Hal.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddSupervisorConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering supervisor services.");

            services.AddScoped<ISupervisor, Supervisor>();

            return services;
        }

        public static IServiceCollection AddRepositoryConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering repositories configuration.");

            services.AddScoped<IWebDriverRepository, WebDriverRepository>();

            return services;
        }

        public static IServiceCollection AddSeleniumServicesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering selenium services configuration.");

            
            services.Configure<ChromeConfigOptions>(options => configuration.GetSection(nameof(ChromeConfigOptions)).Bind(options));
            ChromeConfigOptions webDriverConfigOptions = new();
            configuration.GetSection(nameof(ChromeConfigOptions)).Bind(webDriverConfigOptions);
            string blankTabWindowHandleId = string.Empty;
            services.AddSingleton<IWebDriver, ChromeDriver>(opt =>
            {
                ChromeOptions options = new();
                foreach (string addArgument in webDriverConfigOptions.AddArguments)
                {
                    options.AddArgument(addArgument);
                }

                options.AddArgument(@$"user-data-dir={webDriverConfigOptions.ChromeUserDirectory}\{webDriverConfigOptions.DefaultProfile}");
                ChromeDriver drv = new ChromeDriver(options);
                blankTabWindowHandleId = drv.CurrentWindowHandle;
                return drv;
            });            
            services.AddSingleton<IDefaultTabWebDriver, DefaultTabWebDriver>(opt =>
            {
                DefaultTabWebDriver defaultTab = new(blankTabWindowHandleId);
                return defaultTab;
            });
            services.AddSingleton<IFileManager, FileManager>();
            services.AddScoped<ILinkedInLoginPage, LinkedInLoginPage>();
            services.AddScoped<ILinkedInPage, LinkedInPage>();
            services.AddScoped<ILinkedInHomePage, LinkedInHomePage>();
            services.AddScoped<WebDriverWait>(opt =>
            {
                IWebDriver drv = opt.GetRequiredService<IWebDriver>();
                WebDriverWait wait = new WebDriverWait(drv, TimeSpan.FromSeconds(webDriverConfigOptions.WebDriverWaitFromSeconds));
                return wait;
            });

            services.AddScoped<IHalAuthProvider, HalAuthProvider>();
            services.AddSingleton<IHalIdentity, HalIdentity>(opt =>
            {
                return new HalIdentity(Guid.NewGuid().ToString());
            });

            services.AddScoped<IWebDriverService, WebDriverService>();
            services.AddScoped<IWebDriverProvider, WebDriverProvider>();

            return services;
        }

        public static IServiceCollection AddJsonWebTokenConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Jwt services.");

            IConfigurationSection jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // retrieve private key from user secrets or azure vault
            string privateKey = "test"; //configuration[ApiConstants.VaultKeys.JwtSecret];
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

    }
}
