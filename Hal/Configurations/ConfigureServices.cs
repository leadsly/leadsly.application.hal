using Domain;
using Domain.Facades;
using Domain.Facades.Interfaces;
using Domain.OptionsJsonModels;
using Domain.PhaseConsumers.FollowUpMessageHandlers;
using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingHandler;
using Domain.PhaseConsumers.RestartApplicationHandler;
using Domain.PhaseConsumers.ScanProspectsForRepliesHandlers;
using Domain.PhaseHandlers.FollowUpMessageHandlers;
using Domain.PhaseHandlers.MonitorForNewConnectionsHandler;
using Domain.PhaseHandlers.NetworkingHandler;
using Domain.PhaseHandlers.ProspectListHandler;
using Domain.PhaseHandlers.RestartApplicationHandler;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Domain.PhaseHandlers.SendConnectionsHandler;
using Domain.POMs;
using Domain.POMs.Controls;
using Domain.POMs.Dialogs;
using Domain.POMs.Pages;
using Domain.Providers;
using Domain.Providers.Campaigns;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.RabbitMQ;
using Domain.RabbitMQ.Interfaces;
using Domain.Repositories;
using Domain.Serializers;
using Domain.Serializers.Interfaces;
using Domain.Services;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Networking;
using Domain.Services.Interfaces.SendConnections;
using Domain.Services.Networking;
using Domain.Services.SendConnectionsModals;
using Domain.Supervisor;
using Hal.OptionsJsonModels;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Repositories;
using Leadsly.Application.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PageObjects;
using PageObjects.Controls;
using PageObjects.Dialogs.SearchPageDialogs;
using PageObjects.Pages;
using Serilog;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
        {
            Log.Information("Registering command handlers.");

            // Commands fired to start consuming
            services.AddScoped<HalConsumingCommandHandlerDecorator<RestartApplicationConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<FollowUpMessageConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<MonitorForNewConnectionsConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<NetworkingConnectionsConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand>>();

            // Commands fired to start processing the given phase
            services.AddScoped<HalWorkCommandHandlerDecorator<RestartApplicationCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<FollowUpMessageCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<ProspectListCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<SendConnectionsCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<NetworkingCommand>>();

            // Handlers for starting consumption
            services.AddScoped<IConsumeCommandHandler<RestartApplicationConsumerCommand>, RestartApplicationConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<FollowUpMessageConsumerCommand>, FollowUpMessageConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<MonitorForNewConnectionsConsumerCommand>, MonitorForNewConnectionsConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<NetworkingConnectionsConsumerCommand>, NetworkingConnectionsConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<ScanProspectsForRepliesConsumerCommand>, ScanProspectsForRepliesConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<NetworkingConsumerCommand>, NetworkingConsumerCommandHandler>();

            // Handlers for processing the given phase
            services.AddScoped<ICommandHandler<RestartApplicationCommand>, RestartApplicationCommandHandler>();
            services.AddScoped<ICommandHandler<FollowUpMessageCommand>, FollowUpMessageCommandHandler>();
            services.AddScoped<ICommandHandler<MonitorForNewConnectionsCommand>, MonitorForNewConnectionsCommandHandler>();
            services.AddScoped<ICommandHandler<ProspectListCommand>, ProspectListCommandHandler>();
            services.AddScoped<ICommandHandler<DeepScanProspectsForRepliesCommand>, DeepScanProspectsForRepliesCommandHandler>();
            services.AddScoped<ICommandHandler<ScanProspectsForRepliesCommand>, ScanProspectsForRepliesCommandHandler>();
            services.AddScoped<ICommandHandler<SendConnectionsCommand>, SendConnectionsCommandHandler>();
            services.AddScoped<ICommandHandler<CheckOffHoursNewConnectionsCommand>, CheckOffHoursNewConnectionsCommandHandler>();
            services.AddScoped<ICommandHandler<NetworkingCommand>, NetworkingCommandHandler>();

            return services;
        }

        public static IServiceCollection AddSeleniumServicesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering selenium services configuration.");

            services.Configure<AppServerConfigOptions>(options => configuration.GetSection(nameof(AppServerConfigOptions)).Bind(options));

            services.Configure<SidecartServerConfigOptions>(options => configuration.GetSection(nameof(SidecartServerConfigOptions)).Bind(options));

            services.Configure<WebDriverConfigOptions>(options => configuration.GetSection(nameof(WebDriverConfigOptions)).Bind(options));
            WebDriverConfigOptions webDriverConfigOptions = new();
            configuration.GetSection(nameof(WebDriverConfigOptions)).Bind(webDriverConfigOptions);

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IHalIdentity, HalIdentity>(opt =>
            {
                string halId = Environment.GetEnvironmentVariable("HAL_ID");
                if (halId == string.Empty || halId == null)
                {
                    Log.Warning("HAL_ID enviornment variable was not found or its value was not set");
                    throw new ArgumentNullException("HAL_ID env variable was null but is expected to be set");
                }

                return new HalIdentity(halId);
            });

            services.AddScoped<ICrawlProspectsService, CrawlProspectsService>();
            services.AddScoped<ICampaignProspectsService, CampaignProspectsService>();
            services.AddScoped<IScreenHouseKeeperService, ScreenHouseKeeperService>();

            return services;
        }

        public static IServiceCollection AddFacadesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering facades configuration.");

            services.AddScoped<ICampaignPhaseFacade, CampaignPhaseFacade>();
            services.AddScoped<ILinkedInPageFacade, LinkedInPageFacade>();

            return services;
        }

        public static IServiceCollection AddSerializersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering serializers configuration.");

            services.AddScoped<IRabbitMQSerializer, RabbitMQSerializer>();
            services.AddScoped<ICampaignSerializer, CampaignSerializer>();

            return services;
        }

        public static IServiceCollection AddRabbitMQConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering rabbit mq services configuration.");

            services.Configure<RabbitMQConfigOptions>(options => configuration.GetSection(nameof(RabbitMQConfigOptions)).Bind(options));
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton(s =>
            {
                ObjectPoolProvider provider = s.GetRequiredService<ObjectPoolProvider>();
                IOptions<RabbitMQConfigOptions> rabbitMQConfigOptions = s.GetRequiredService<IOptions<RabbitMQConfigOptions>>();
                string halId = Environment.GetEnvironmentVariable("HAL_ID");
                if (halId == string.Empty || halId == null)
                {
                    Log.Warning("HAL_ID enviornment variable was not found or its value was not set");
                    throw new ArgumentNullException("HAL_ID env variable was null but is expected to be set");
                }
                return provider.Create(new RabbitModelPooledObjectPolicy(rabbitMQConfigOptions.Value, halId));
            });

            return services;
        }

        public static IServiceCollection AddPageObjectModelsConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering page object models services configuration.");

            services.AddScoped<ILinkedInLoginPage, LinkedInLoginPage>();
            services.AddScoped<ILinkedInPage, LinkedInPage>();
            services.AddScoped<ILinkedInHomePage, LinkedInHomePage>();
            services.AddScoped<ILinkedInMessagingPage, LinkedInMessagingPage>();
            services.AddScoped<ILinkedInMyNetworkPage, LinkedInMyNetworkPage>();
            services.AddScoped<ILinkedInNavBar, LinkedInNavBar>();
            services.AddScoped<ILinkedInSearchPage, LinkedInSearchPage>();
            services.AddScoped<ILinkedInNotificationsPage, LinkedInNotificationsPage>();
            services.AddScoped<IAcceptedInvitiationsView, AcceptedInvitationsView>();
            services.AddScoped<IConnectionsView, ConnectionsView>();
            services.AddScoped<IConversationCards, ConversationCards>();
            services.AddScoped<ISearchPageDialogManager, SearchPageDialogManager>();
            services.AddScoped<IHowDoYouKnowDialog, HowDoYouKnowDialog>();
            services.AddScoped<ICustomizeYourInvitationDialog, CustomizeYourInvitationDialog>();

            return services;
        }

        public static IServiceCollection AddProvidersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering providers configuration.");

            services.AddScoped<IHalAuthProvider, HalAuthProvider>();
            services.AddScoped<IWebDriverProvider, WebDriverProvider>();
            services.AddScoped<IWebDriverManagerProvider, WebDriverManagerProvider>();
            services.AddScoped<IFollowUpMessagesProvider, FollowUpMessagesProvider>();
            services.AddScoped<IMonitorForNewProspectsProvider, MonitorForNewProspectsProvider>();
            services.AddScoped<IHalOperationConfigurationProvider, HalOperationConfigurationProvider>();
            services.AddScoped<IProspectListProvider, ProspectListProvider>();
            services.AddScoped<ICampaignProvider, CampaignProvider>();
            services.AddScoped<ISendConnectionsProvider, SendConnectionsProvider>();
            services.AddScoped<IScanProspectsForRepliesProvider, ScanProspectsForRepliesProvider>();
            services.AddScoped<IDeepScanProspectsForRepliesProvider, DeepScanProspectsForRepliesProvider>();
            services.AddScoped<IPhaseDataProcessingProvider, PhaseDataProcessingProvider>();
            services.AddScoped<ITriggerPhaseProvider, TriggerPhaseProvider>();
            services.AddScoped<INetworkingProvider, NetworkingProvider>();

            return services;
        }

        public static IServiceCollection AddServicesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering services configuration.");

            services.AddHttpClient<IPhaseDataProcessingService, PhaseDataProcessingService>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<ICampaignService, CampaignService>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<ITriggerPhaseService, TriggerPhaseService>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<ILeadslyGridSidecartService, LeadslyGridSidecartService>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddScoped<IWebDriverService, WebDriverService>();
            services.AddScoped<ITimestampService, TimestampService>();
            services.AddScoped<IPhaseEventHandlerService, PhaseEventHandlerService>();
            services.AddScoped<IRabbitMQManager, RabbitMQManager>();
            services.AddScoped<IWebDriverUtilities, WebDriverUtilities>();
            services.AddScoped<IHumanBehaviorService, HumanBehaviorService>();
            services.AddSingleton<IConsumingService, ConsumingService>();
            services.AddSingleton<Random>();
            services.AddScoped<IUrlService, UrlService>();
            services.AddScoped<IHowDoYouKnowModalService, HowDoYouKnowModalService>();
            services.AddScoped<ICustomizeInvitationModalService, CustomizeInvitationModalService>();
            services.AddScoped<ISendConnectionsService, SendConnectionsService>();

            return services;
        }

        public static IServiceCollection AddJsonWebTokenConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Jwt services.");

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
                ValidateIssuer = false,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                // do not validate audience because audience is created for the frontend app user not hal, but for now this is ok.
                // will need to be refactored in the future.
                ValidateAudience = false,
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

            //services.AddAuthorization(options =>
            //{
            //    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
            //                    .AddRequirements(new DenyAnonymousAuthorizationRequirement())
            //                    .Build();
            //});

            return services;
        }

        public static IServiceCollection AddRemoveNull204FormatterConfigration(this IServiceCollection services)
        {
            Log.Information("Configuring output formatters.");

            services.AddControllers(opt =>
            {
                // remove formatter that turns nulls into 204 - Angular http client treats 204s as failed requests
                HttpNoContentOutputFormatter noContentFormatter = opt.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if (noContentFormatter != null)
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

        public static IServiceCollection AddHangfireConfig(this IServiceCollection services, string defaultConnection)
        {
            Log.Information("Registering hangfire services.");

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            GlobalConfiguration.Configuration.UseSerilogLogProvider();

            PostgreSqlStorageOptions options = new PostgreSqlStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                // JobExpirationCheckInterval = TimeSpan.FromHours(24)
            };

            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(defaultConnection, options);
                config.UseRecommendedSerializerSettings();
            }).AddHangfireServer();

            return services;
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
