using DC_bot.Commands.Music;
using DC_bot.Commands.Queue;
using DC_bot.Commands.Utility;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Presentation;
using DC_bot.IO;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Service.Music.ProgressiveTimer;
using DC_bot.Service.Presentation;
using DC_bot.Wrapper;
using Lavalink4NET.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Startup;

internal static class BotServiceProviderFactory
{
    public static ServiceProvider Create(BotRuntimeSettings settings)
    {
        return Create(settings.BotSettings, settings.LavalinkSettings, settings.PostgresConnectionString);
    }

    public static ServiceProvider Create(
        BotSettings botSettings,
        LavalinkSettings lavalinkSettings,
        string? postgresConnectionString = null)
    {
        postgresConnectionString ??= BotConfigurationLoader.BuildPostgresConnectionString();

        return new ServiceCollection()
            .ConfigureLavalink(options =>
            {
                var httpScheme = lavalinkSettings.Secured ? "https" : "http";
                var wsScheme = lavalinkSettings.Secured ? "wss" : "ws";
                options.BaseAddress = new Uri($"{httpScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}");
                options.WebSocketUri =
                    new Uri($"{wsScheme}://{lavalinkSettings.Hostname}:{lavalinkSettings.Port}/v4/websocket");
                options.Passphrase = lavalinkSettings.Password;
            })
            .AddLavalink()
            .AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); })
            .AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(postgresConnectionString))
            .AddSingleton<IGuildDataRepository, GuildDataRepository>()
            .AddSingleton<IPlaybackStateRepository, PlaybackStateRepository>()
            .AddSingleton<IQueueRepository, QueueRepository>()
            .AddSingleton<IRepeatListRepository, RepeatListRepository>()
            .AddSingleton(botSettings)
            .AddSingleton<IFileSystem, PhysicalFileSystem>()
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>()))
            .AddSingleton<BotService>()
            .AddSingleton<ReactionHandler>()
            .AddSingleton<ICommand, TagCommand>()
            .AddSingleton<CommandHandlerService>()
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
            .AddSingleton<ICommand, RepeatListCommand>()
            .AddSingleton<IResponseBuilder, ResponseBuilder>()
            .AddSingleton<ICommandHelper, CommandValidationService>()
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
            .AddSingleton<IValidationService, ValidationService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IUserValidationService, ValidationService>()
            .AddSingleton<IProgressiveTimerService, ProgressiveTimerService>()
            .AddSingleton<ITrackSearchResolverService, TrackSearchResolverService>()
            .BuildServiceProvider();
    }
}
