using Microsoft.Extensions.Configuration;
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
using RabbitMQ.Client;
using Domain.Deserializers.Interfaces;
using Domain.Deserializers;
using Domain.Facades;
using Domain.Facades.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;

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
            services.AddScoped<IRabbitMQRepository, RabbitMQRepository>();

            return services;
        }

        public static IServiceCollection AddSeleniumServicesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering selenium services configuration.");
            
            services.Configure<WebDriverConfigOptions>(options => configuration.GetSection(nameof(WebDriverConfigOptions)).Bind(options));
            WebDriverConfigOptions webDriverConfigOptions = new();
            configuration.GetSection(nameof(WebDriverConfigOptions)).Bind(webDriverConfigOptions);
                
            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IHalIdentity, HalIdentity>(opt =>
            {
                string halId = Environment.GetEnvironmentVariable("HAL_ID", EnvironmentVariableTarget.User) ?? "HAL_ID_NOT_FOUND";
                if (halId == "HAL_ID_NOT_FOUND")
                {
                    Log.Warning("HAL_ID enviornment variable was not found or its value was not set");
                    throw new ArgumentNullException("HAL_ID env variable was null but is expected to be set");
                }                    

                return new HalIdentity(halId);
            });                       
            
            return services;
        }

        public static IServiceCollection AddFacadesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering facades configuration.");

            services.AddScoped<IDeserializerFacade, DeserializerFacade>();
            services.AddScoped<ICampaignPhaseFacade, CampaignPhaseFacade>();

            return services;
        }

        public static IServiceCollection AddSerializersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering serializers configuration.");

            services.AddScoped<IFollowUpMessagesDeserializer, FollowUpMessagesDeserializer>();

            return services;
        }

        public static IServiceCollection AddRabbitMQConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering rabbit mq services configuration.");

            services.Configure<RabbitMQConfigOptions>(options => configuration.GetSection(nameof(RabbitMQConfigOptions)).Bind(options));

            return services;
        }

        public static IServiceCollection AddPageObjectModelsConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering page object models services configuration.");

            services.AddScoped<ILinkedInLoginPage, LinkedInLoginPage>();
            services.AddScoped<ILinkedInPage, LinkedInPage>();
            services.AddScoped<ILinkedInHomePage, LinkedInHomePage>();
            services.AddScoped<ILinkedInMessagingPage, LinkedInMessagingPage>();

            return services;
        }

        public static IServiceCollection AddProvidersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering providers configuration.");

            services.AddScoped<IHalAuthProvider, HalAuthProvider>();
            services.AddScoped<IWebDriverProvider, WebDriverProvider>();
            services.AddScoped<IWebDriverManagerProvider, WebDriverManagerProvider>();

            return services;
        }

        public static IServiceCollection AddServicesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering services configuration.");

            services.AddScoped<IWebDriverService, WebDriverService>();
            services.AddScoped<IConsumingService, ConsumingService>();
            services.AddScoped<ICampaignManagerService, CampaignManagerService>();

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
