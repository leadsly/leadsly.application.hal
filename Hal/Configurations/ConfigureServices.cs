using Domain;
using Domain.Executors;
using Domain.Executors.FollowUpMessage;
using Domain.Executors.MonitorForNewConnections;
using Domain.Executors.Networking;
using Domain.Executors.ScanProspectsForReplies;
using Domain.Facades;
using Domain.Facades.Interfaces;
using Domain.InstructionSets;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages;
using Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessageContent;
using Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessageContent.Interfaces;
using Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessagesContent;
using Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessagesContent.Interfaces;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount;
using Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.CreateNewMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage.Interfaces;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Interactions.FollowUpMessage.EnterProspectName.Interfaces;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded.Interfaces;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount.Interfaces;
using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.ConnectWithProspect.Interfaces;
using Domain.Interactions.Networking.Decorators;
using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.GatherProspects.Interfaces;
using Domain.Interactions.Networking.GetTotalSearchResults;
using Domain.Interactions.Networking.GetTotalSearchResults.Interfaces;
using Domain.Interactions.Networking.GoToTheNextPage;
using Domain.Interactions.Networking.GoToTheNextPage.Interfaces;
using Domain.Interactions.Networking.IsLastPage;
using Domain.Interactions.Networking.IsLastPage.Interfaces;
using Domain.Interactions.Networking.IsNextButtonDisabled;
using Domain.Interactions.Networking.IsNextButtonDisabled.Interfaces;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.Interactions.Networking.SearchResultsLimit;
using Domain.Interactions.Networking.SearchResultsLimit.Interfaces;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages.Interfaces;
using Domain.Interactions.Shared.CloseAllConversations;
using Domain.Interactions.Shared.CloseAllConversations.Interfaces;
using Domain.Interactions.Shared.RefreshBrowser;
using Domain.Interactions.Shared.RefreshBrowser.Interfaces;
using Domain.MQ;
using Domain.MQ.EventHandlers;
using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Interfaces;
using Domain.MQ.Messages;
using Domain.MQ.Services;
using Domain.MQ.Services.Interfaces;
using Domain.OptionsJsonModels;
using Domain.Orchestrators;
using Domain.Orchestrators.Interfaces;
using Domain.PhaseConsumers.AllInOneVirtualAssistantHandler;
using Domain.PhaseConsumers.FollowUpMessageHandlers;
using Domain.PhaseConsumers.MonitorForNewConnectionsHandlers;
using Domain.PhaseConsumers.NetworkingHandler;
using Domain.PhaseConsumers.RestartApplicationHandler;
using Domain.PhaseConsumers.ScanProspectsForRepliesHandlers;
using Domain.PhaseHandlers.AllInOneVirtualAssistantHandler;
using Domain.PhaseHandlers.FollowUpMessageHandlers;
using Domain.PhaseHandlers.MonitorForNewConnectionsHandler;
using Domain.PhaseHandlers.NetworkingHandler;
using Domain.PhaseHandlers.RestartApplicationHandler;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Domain.POMs;
using Domain.POMs.Controls;
using Domain.POMs.Dialogs;
using Domain.POMs.Pages;
using Domain.Providers;
using Domain.Providers.Interfaces;
using Domain.Repositories;
using Domain.Services;
using Domain.Services.Api;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Domain.Services.Interfaces.POMs;
using Domain.Services.POMs;
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
            services.AddScoped<HalConsumingCommandHandlerDecorator<NetworkingConsumerCommand>>();
            services.AddScoped<HalConsumingCommandHandlerDecorator<ScanProspectsForRepliesConsumerCommand>>();

            // Commands fired to start processing the given phase
            services.AddScoped<HalWorkCommandHandlerDecorator<RestartApplicationCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<FollowUpMessageCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand>>();
            services.AddScoped<HalWorkCommandHandlerDecorator<NetworkingCommand>>();

            // Handlers for starting consumption
            services.AddScoped<IConsumeCommandHandler<RestartApplicationConsumerCommand>, RestartApplicationConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<FollowUpMessageConsumerCommand>, FollowUpMessageConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<MonitorForNewConnectionsConsumerCommand>, MonitorForNewConnectionsConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<ScanProspectsForRepliesConsumerCommand>, ScanProspectsForRepliesConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<NetworkingConsumerCommand>, NetworkingConsumerCommandHandler>();
            services.AddScoped<IConsumeCommandHandler<AllInOneVirtualAssistantConsumerCommand>, AllInOneVirtualAssistantConsumerCommandHandler>();

            // Handlers for processing the given phase
            services.AddScoped<ICommandHandler<RestartApplicationCommand>, RestartApplicationCommandHandler>();
            services.AddScoped<ICommandHandler<FollowUpMessageCommand>, FollowUpMessageCommandHandler>();
            services.AddScoped<ICommandHandler<MonitorForNewConnectionsCommand>, MonitorForNewConnectionsCommandHandler>();
            services.AddScoped<ICommandHandler<DeepScanProspectsForRepliesCommand>, DeepScanProspectsForRepliesCommandHandler>();
            services.AddScoped<ICommandHandler<ScanProspectsForRepliesCommand>, ScanProspectsForRepliesCommandHandler>();
            services.AddScoped<ICommandHandler<CheckOffHoursNewConnectionsCommand>, CheckOffHoursNewConnectionsCommandHandler>();
            services.AddScoped<ICommandHandler<NetworkingCommand>, NetworkingCommandHandler>();
            services.AddScoped<ICommandHandler<AllInOneVirtualAssistantCommand>, AllInOneVirtualAssistantCommandHandler>();

            return services;
        }

        public static IServiceCollection AddJsonOptions(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering JsonOptions services configuration.");

            services.Configure<FeatureFlagsOptions>(options => configuration.GetSection(nameof(FeatureFlagsOptions)).Bind(options));

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

            services.AddScoped<IScreenHouseKeeperServicePOM, ScreenHouseKeeperServicePOM>();
            services.AddScoped<ISearchPageFooterServicePOM, SearchPageFooterServicePOM>();
            services.AddScoped<ICustomizeInvitationModalServicePOM, CustomizeInvitationModalServicePOM>();
            services.AddScoped<IHowDoYouKnowModalServicePOM, HowDoYouKnowModalServicePOM>();
            services.AddScoped<IDeepScanProspectsServicePOM, DeepScanProspectsServicePOM>();
            services.AddScoped<IScanProspectsServicePOM, ScanProspectsServicePOM>();
            services.AddScoped<IFollowUpMessageServicePOM, FollowUpMessageServicePOM>();
            services.AddScoped<ICheckOffHoursNewConnectionsServicePOM, CheckOffHoursNewConnectionsServicePOM>();
            services.AddScoped<IMonitorForNewConnectionsServicePOM, MonitorForNewConnectionsServicePOM>();
            services.AddScoped<IMessageListBubbleServicePOM, MessageListBubbleServicePOM>();

            return services;
        }

        public static IServiceCollection AddServicesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering services configuration.");

            services.AddHttpClient<ILeadslyGridSidecartService, LeadslyGridSidecartService>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<INetworkingServiceApi, NetworkingServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<IScanProspectsForRepliesServiceApi, ScanProspectsForRepliesServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<IDeepScanProspectsForRepliesServiceApi, DeepScanProspectsForRepliesServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<ISendFollowUpMessageServiceApi, SendFollowUpMessageServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<IMonitorProspectsForNewConnectionsServiceApi, MonitorProspectsForNewConnectionsServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddHttpClient<IAllInOneVirtualAssistantServiceApi, AllInOneVirtualAssistantServiceApi>(opt =>
            {
                opt.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddScoped<IWebDriverService, WebDriverService>();
            services.AddScoped<ITimestampService, TimestampService>();
            services.AddScoped<IRabbitMQManager, RabbitMQManager>();
            services.AddScoped<IWebDriverUtilities, WebDriverUtilities>();
            services.AddScoped<IHumanBehaviorService, HumanBehaviorService>();
            services.AddSingleton<IConsumingService, ConsumingService>();
            services.AddSingleton<Random>();
            services.AddScoped<IUrlService, UrlService>();
            services.AddScoped<INetworkingService, NetworkingService>();
            services.AddScoped<IScanProspectsService, ScanProspectsService>();
            services.AddScoped<IDeepScanProspectsService, DeepScanProspectsService>();
            services.AddScoped<IFollowUpMessageService, FollowUpMessageService>();
            services.AddScoped<IMonitorForNewConnectionsService, MonitorForNewConnectionsService>();
            services.AddScoped<IAllInOneVirtualAssistantService, AllInOneVirtualAssistantService>();

            return services;
        }

        public static IServiceCollection AddRabbitMQEventHandlers(this IServiceCollection services)
        {
            Log.Information("Registering RabbitMQ event handlers.");

            services.AddScoped<IFollowUpMessageEventHandler, FollowUpMessageEventHandler>();
            services.AddScoped<IMonitorForNewAcceptedConnectionsEventHandler, MonitorForNewAcceptedConnectionsEventHandler>();
            services.AddScoped<INetworkingEventHandler, NetworkingEventHandler>();
            services.AddScoped<IRestartApplicationEventHandler, RestartApplicationEventHandler>();
            services.AddScoped<IScanProspectsForRepliesEventHandler, ScanProspectsForRepliesEventHandler>();
            services.AddScoped<IAllInOneVirtualAssistantEventHandler, AllInOneVirtualAssistantEventHandler>();

            return services;
        }

        public static IServiceCollection AddOrchestratorServices(this IServiceCollection services)
        {
            Log.Information("Registering orchestrator services.");

            services.AddScoped<IAllInOneVirtualAssistantPhaseMetaOrchestrator, AllInOneVirtualAssistantPhaseMetaOrchestrator>();

            services.AddScoped<IFollowUpMessagePhaseOrchestrator, FollowUpMessagePhaseOrchestrator>();
            services.AddScoped<INetworkingPhaseOrchestrator, NetworkingPhaseOrchestrator>();
            services.AddScoped<IDeepScanProspectsForRepliesPhaseOrchestrator, DeepScanProspectsForRepliesOrchestrator>();
            services.AddScoped<IScanProspectsForRepliesPhaseOrchestrator, ScanProspectsForRepliesPhaseOrchestrator>();
            services.AddScoped<ICheckOffHoursNewConnectionsPhaseOrchestrator, CheckOffHoursNewConnectionsPhaseOrchestrator>();
            services.AddScoped<IMonitorForNewConnectionsPhaseOrchestrator, MonitorForNewConnectionsPhaseOrchestrator>();

            return services;
        }

        public static IServiceCollection AddInteractionSets(this IServiceCollection services)
        {
            Log.Information("Registering intersection sets services.");

            services.AddScoped<INetworkingInstructionSet, NetworkingInstructionSet>();
            services.AddScoped<ICheckForNewConnectionsFromOffHoursInstructionSet, CheckForNewConnectionsFromOffHoursInstructionSet>();
            services.AddScoped<IDeepScanInstructionSet, DeepScanInstructionSet>();
            services.AddScoped<IFollowUpMessageInstructionSet, FollowUpMessageInstructionSet>();

            return services;
        }

        public static IServiceCollection AddInteractionHandlers(this IServiceCollection services)
        {
            Log.Information("Registering interaction handlers.");

            ////////////////////////////////////////////////////////////
            /// DeepScanProspectsForReplies Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<ICheckMessagesHistoryInteractionHandler, CheckMessagesHistoryInteractionHandler>();
            services.AddScoped<IClearMessagingSearchCriteriaInteractionHandler, ClearMessagingSearchCriteriaInteractionHandler>();
            services.AddScoped<IEnterSearchMessageCriteriaInteractionHandler, EnterSearchMessageCriteriaInteractionHandler>();
            services.AddScoped<IGetProspectsMessageItemInteractionHandler, GetProspectsMessageItemInteractionHandler>();
            services.AddScoped<IGetAllVisibleConversationCountInteractionHandler, GetAllVisibleConversationCountInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// FollowUpMessage Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<ICreateNewMessageInteractionHandler, CreateNewMessageInteractionHandler>();
            services.AddScoped<IEnterMessageInteractionHandler, EnterMessageInteractionHandler>();
            services.AddScoped<IEnterProspectNameInteractionHandler, EnterProspectNameInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// Networking Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<RetryConnectWithProspectHandlerDecorator>();
            services.AddScoped<RetryGatherProspectsHandlerDecorator>();
            services.AddScoped<IConnectWithProspectInteractionHandler, ConnectWithProspectInteractionHandler>();
            services.AddScoped<IGatherProspectsInteractionHandler, GatherProspectsInteractionHandler>();
            services.AddScoped<INoResultsFoundInteractionHandler, NoResultsFoundInteractionHandler>();
            services.AddScoped<ISearchResultsLimitInteractionHandler, SearchResultsLimitInteractionHandler>();
            services.AddScoped<IGetTotalSearchResultsInteractionHandler, GetTotalSearchResultsInteractionHandler>();
            services.AddScoped<IGoToTheNextPageInteractionHandler, GoToTheNextPageInteractionHandler>();
            services.AddScoped<IIsLastPageInteractionHandler, IsLastPageInteractionHandler>();
            services.AddScoped<IIsNextButtonDisabledInteractionHandler, IsNextButtonDisabledInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// ScanProspectsForReplies Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces.IGetMessageContentInteractionHandler, Domain.Interactions.ScanProspectsForReplies.GetMessageContent.GetMessageContentInteractionHandler>();
            services.AddScoped<IGetNewMessagesInteractionHandler, GetNewMessagesInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// CheckOffHoursNewConnections Interactions
            ///////////////////////////////////////////////////////////            
            services.AddScoped<IGetAllRecentlyAddedSinceInteractionHandler, GetAllRecentlyAddedSinceInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// MonitorForNewConnections Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<IGetAllRecentlyAddedInteractionHandler, GetAllRecentlyAddedInteractionHandler>();
            services.AddScoped<IGetConnectionsCountInteractionHandler, GetConnectionsCountInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// AllInOneVirtualAssistant Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<IGetAllUnreadMessagesInteractionHandler, GetAllUnreadMessagesInteractionHandler>();
            services.AddScoped<IGetUnreadMessageContentInteractionHandler, GetUnreadMessageContentInteractionHandler>();
            services.AddScoped<IGetUnreadMessagesContentInteractionHandler, GetUnreadMessagesContentInteractionHandler>();
            services.AddScoped<IGetUnreadMessagesContentInteractionHandler, GetUnreadMessagesContentInteractionHandler>();

            ////////////////////////////////////////////////////////////
            /// Shared Interactions
            ///////////////////////////////////////////////////////////
            services.AddScoped<ICloseAllConversationsInteractionHandler, CloseAllConversationsInteractionHandler>();
            services.AddScoped<IRefreshBrowserInteractionHandler, RefreshBrowserInteractionHandler>();

            return services;
        }

        public static IServiceCollection AddMessageExecutorHandlers(this IServiceCollection services)
        {
            Log.Information("Registering message executor handler.");
            services.AddScoped<IMessageExecutorHandler<NetworkingMessageBody>, NetworkingMessageExecutorHandler>();
            services.AddScoped<IMessageExecutorHandler<DeepScanProspectsForRepliesBody>, DeepScanProspectsForRepliesExecutorHandler>();
            services.AddScoped<IMessageExecutorHandler<ScanProspectsForRepliesBody>, ScanProspectsForRepliesExecutorHandler>();
            services.AddScoped<IMessageExecutorHandler<FollowUpMessageBody>, FollowUpMessageExecutorHandler>();
            services.AddScoped<IMessageExecutorHandler<MonitorForNewAcceptedConnectionsBody>, MonitorForNewConnectionsExecutorHandler>();
            services.AddScoped<IMessageExecutorHandler<CheckOffHoursNewConnectionsBody>, CheckOffHoursNewConnectionsExecutorHandler>();

            return services;
        }

        public static IServiceCollection AddFacadesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering facades configuration.");

            services.AddScoped<ILinkedInPageFacade, LinkedInPageFacade>();
            services.AddScoped<INetworkingInteractionFacade, NetworkingInteractionFacade>();
            services.AddScoped<IDeepScanProspectsInteractionFacade, DeepScanProspectsInteractionFacade>();
            services.AddScoped<IFollowUpMessageInteractionFacade, FollowUpMessageInteractionFacade>();
            services.AddScoped<IMonitorForConnectionsInteractionFacade, MonitorForConnectionsInteractionFacade>();
            services.AddScoped<IScanProspectsForRepliesInteractionFacade, ScanProspectsForRepliesInteractionFacade>();
            services.AddScoped<IDeepScanProspectsInteractionFacade, DeepScanProspectsInteractionFacade>();
            services.AddScoped<IAllInOneOrchestratorsFacade, AllInOneOrchestratorsFacade>();

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

        public static IServiceCollection AddRabbitMQServices(this IServiceCollection services)
        {
            Log.Information("Registering rabbit mq services");

            services.AddScoped<IGetMQMessagesService, GetMQMessagesService>();

            return services;
        }

        public static IServiceCollection AddPageObjectModelsConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering page object models services configuration.");

            services.AddScoped<ILinkedInLoginPage, LinkedInLoginPage>();
            services.AddScoped<ILinkedInPage, LinkedInPage>();
            services.AddScoped<ILinkedInHomePage, LinkedInHomePage>();
            services.AddScoped<ILinkedInMessagingPage, LinkedInMessagingPage>();
            services.AddScoped<ILinkedInSearchPage, LinkedInSearchPage>();
            services.AddScoped<IConnectionsView, ConnectionsView>();
            services.AddScoped<IConversationCards, ConversationCards>();
            services.AddScoped<ISearchPageDialogManager, SearchPageDialogManager>();
            services.AddScoped<IHowDoYouKnowDialog, HowDoYouKnowDialog>();
            services.AddScoped<ICustomizeYourInvitationDialog, CustomizeYourInvitationDialog>();
            services.AddScoped<ISearchResultsFooter, SearchResultsFooter>();
            services.AddScoped<IMessageListBubble, MessageListBubble>();

            return services;
        }

        public static IServiceCollection AddProvidersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering providers configuration.");

            services.AddScoped<IWebDriverProvider, WebDriverProvider>();

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
                        return System.Threading.Tasks.Task.CompletedTask;
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
