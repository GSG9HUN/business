using DC_bot.Commands.SlashCommands.Music;
using DC_bot.Commands.SlashCommands.Queue;
using DC_bot.Commands.SlashCommands.Utility;
using DC_bot.Commands.TextCommands.Music;
using DC_bot.Commands.TextCommands.Queue;
using DC_bot.Commands.TextCommands.Utility;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Interface.Service.SlashCommands;
using DC_bot.Helper.Factory;
using DC_bot.IO;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Service.Music.ProgressiveTimer;
using DC_bot.Service.Presentation;
using DC_bot.Service.SlashCommands;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Extensions;
using Lavalink4NET.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup;

internal static class BotServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordRuntime(this IServiceCollection services, string discordToken)
    {
        return services
            .AddDiscordClient(discordToken, DiscordIntents.All)
            .ConfigureEventHandlers(builder =>
            {
                builder.HandleSessionCreated((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnClientReady(client, args));
                builder.HandleGuildAvailable((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnGuildAvailable(client, args));
                builder.HandleMessageCreated((client, args) =>
                    client.ServiceProvider.GetRequiredService<CommandHandlerService>()
                        .HandleEventAsync(client, args));
                builder.HandleMessageReactionAdded((client, args) =>
                    client.ServiceProvider.GetRequiredService<ReactionHandler>()
                        .HandleEventAsync(client, args));
                builder.HandleMessageReactionRemoved((client, args) =>
                    client.ServiceProvider.GetRequiredService<ReactionHandler>()
                        .HandleEventAsync(client, args));
            });
    }

    public static IServiceCollection AddSlashCommandProcessor(this IServiceCollection services)
    {
        return services.AddCommandsExtension((_, extension) =>
        {
            extension.AddCommands(
            [
                typeof(PingSlashCommand),
                typeof(HelpSlashCommand),
                typeof(TagSlashCommand),
                typeof(JoinSlashCommand),
                typeof(PlaySlashCommand),
                typeof(SkipSlashCommand),
                typeof(PauseSlashCommand),
                typeof(ResumeSlashCommand),
                typeof(LeaveSlashCommand),
                typeof(QueueSlashCommand),
                typeof(ShuffleSlashCommand),
                typeof(RepeatSlashCommand),
                typeof(LanguageSlashCommand),
                typeof(ClearSlashCommand)
            ]);
            extension.AddProcessor(new SlashCommandProcessor());
        }, new CommandsConfiguration
        {
            RegisterDefaultCommandProcessors = false
        });
    }

    public static IServiceCollection AddLavalinkRuntime(
        this IServiceCollection services,
        LavalinkSettings lavalinkSettings)
    {
        return services
            .ConfigureLavalink(options =>
            {
                var httpScheme = lavalinkSettings.Secured ? "https" : "http";
                var wsScheme = lavalinkSettings.Secured ? "wss" : "ws";
                options.BaseAddress = new Uri($"{httpScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}");
                options.WebSocketUri =
                    new Uri($"{wsScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}/v4/websocket");
                options.Passphrase = lavalinkSettings.Password;
            })
            .AddLavalink();
    }

    public static IServiceCollection AddBotLogging(this IServiceCollection services)
    {
        return services.AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); });
    }

    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        string postgresConnectionString)
    {
        return services
            .AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(postgresConnectionString))
            .AddSingleton<IGuildDataRepository, GuildDataRepository>()
            .AddSingleton<IPlaybackStateRepository, PlaybackStateRepository>()
            .AddSingleton<IQueueRepository, QueueRepository>()
            .AddSingleton<IRepeatListRepository, RepeatListRepository>();
    }

    public static IServiceCollection AddCoreBotServices(this IServiceCollection services, BotSettings botSettings)
    {
        return services
            .AddSingleton(botSettings)
            .AddSingleton<IFileSystem, PhysicalFileSystem>()
            .AddSingleton<IDiscordMessageFactory, DiscordMessageWrapperFactory>()
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton<BotService>()
            .AddSingleton<ReactionHandler>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommandHelper, CommandValidationService>()
            .AddSingleton<IValidationService, ValidationService>()
            .AddSingleton<IUserValidationService, ValidationService>()
            .AddSingleton<ILocalizationService, LocalizationService>();
    }

    public static IServiceCollection AddSlashCommandServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ISlashCommandExecutor, SlashCommandExecutor>()
            .AddSingleton<ISlashInteractionContextFactory, SlashInteractionContextFactory>()
            .AddTransient<PingSlashCommand>()
            .AddTransient<HelpSlashCommand>()
            .AddTransient<TagSlashCommand>()
            .AddTransient<JoinSlashCommand>()
            .AddTransient<PlaySlashCommand>()
            .AddTransient<SkipSlashCommand>()
            .AddTransient<PauseSlashCommand>()
            .AddTransient<ResumeSlashCommand>()
            .AddTransient<LeaveSlashCommand>()
            .AddTransient<QueueSlashCommand>()
            .AddTransient<ShuffleSlashCommand>()
            .AddTransient<RepeatSlashCommand>()
            .AddTransient<LanguageSlashCommand>()
            .AddTransient<ClearSlashCommand>();
    }

    public static IServiceCollection AddTextCommands(this IServiceCollection services)
    {
        return services
            .AddSingleton<ICommand, TagCommand>()
            .AddSingleton<ICommand, JoinCommand>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<ICommand, PlayCommand>()
            .AddSingleton<ICommand, SkipCommand>()
            .AddSingleton<ICommand, ClearCommand>()
            .AddSingleton<ICommand, LeaveCommand>()
            .AddSingleton<ICommand, PauseCommand>()
            .AddSingleton<ICommand, ResumeCommand>()
            .AddSingleton<ICommand, RepeatCommand>()
            .AddSingleton<ICommand, ShuffleCommand>()
            .AddSingleton<ICommand, LanguageCommand>()
            .AddSingleton<ICommand, ViewQueueCommand>()
            .AddSingleton<ICommand, RepeatListCommand>();
    }

    public static IServiceCollection AddMusicServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRepeatService, RepeatService>()
            .AddSingleton<ICurrentTrackService, CurrentTrackService>()
            .AddSingleton<ITrackNotificationService, TrackNotificationService>()
            .AddSingleton<ITrackFormatterService, TrackFormatterService>()
            .AddSingleton<IPlayerConnectionService, PlayerConnectionService>()
            .AddSingleton<ILavalinkNodeConnectionService, LavalinkNodeConnectionService>()
            .AddSingleton<IPlaybackEventHandlerService, PlaybackEventHandlerService>()
            .AddSingleton<IPlaybackControlService, PlaybackControlService>()
            .AddSingleton<IPlaybackRequestService, PlaybackRequestService>()
            .AddSingleton<ITrackPlaybackService, TrackPlaybackService>()
            .AddSingleton<ITrackEndedHandlerService, TrackEndedHandlerService>()
            .AddSingleton<ILavaLinkService, LavaLinkService>()
            .AddSingleton<IMusicQueueService, MusicQueueService>()
            .AddSingleton<IProgressiveTimerService, ProgressiveTimerService>()
            .AddSingleton<ITrackSearchResolverService, TrackSearchResolverService>();
    }
}
