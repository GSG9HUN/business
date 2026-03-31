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
using DC_bot.Interface.Service.Presentation;
using DC_bot.IO;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using DC_bot.Service.Music.ProgressiveTimer;
using DC_bot.Service.Presentation;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot;

internal class Program
{
    private static async Task Main()
    {
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (!File.Exists(envPath))
        {
            Console.WriteLine("Please provide .env file.");
            return;
        }

        Env.Load(envPath);
        await new Program().RunBotAsync();
    }

    private async Task RunBotAsync()
    {
        static string? GetEnv(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().Trim('"');
        }

        var botSettings = new BotSettings
        {
            Token = GetEnv("DISCORD_TOKEN"),
            Prefix = GetEnv("BOT_PREFIX")
        };

        if (string.IsNullOrWhiteSpace(botSettings.Token))
        {
            Console.WriteLine("DISCORD_TOKEN is not set in the environment variables.");
            return;
        }

        var lavalinkHost = GetEnv("LAVALINK_HOSTNAME");
        if (string.IsNullOrWhiteSpace(lavalinkHost))
        {
            Console.WriteLine("LAVALINK_HOSTNAME is not set in the environment variables.");
            return;
        }

        var lavaLinkSettings = new LavalinkSettings
        {
            Hostname = lavalinkHost,
            Port = int.TryParse(GetEnv("LAVALINK_PORT"), out var port) ? port : 2333,
            Secured = string.Equals(GetEnv("LAVALINK_SECURED"), "true", StringComparison.OrdinalIgnoreCase),
            Password = GetEnv("LAVALINK_PASSWORD") ?? string.Empty
        };

        var services = ConfigureServices(botSettings, lavaLinkSettings);
        var botService = services.GetRequiredService<BotService>();

        RegisterSlashCommands(services);
        RegisterHandlers(services);

        await botService.StartAsync();
    }

    private static ServiceProvider ConfigureServices(BotSettings botSettings, LavalinkSettings lavaLinkSettings)
    {
        var services = new ServiceCollection()
            .ConfigureLavalink(options =>
            {
                var httpScheme = lavaLinkSettings.Secured ? "https" : "http";
                var wsScheme = lavaLinkSettings.Secured ? "wss" : "ws";
                var baseAddress = new Uri($"{httpScheme}://{lavaLinkSettings.Hostname}:{lavaLinkSettings.Port}");
                var webSocketUri =
                    new Uri($"{wsScheme}://{lavaLinkSettings.Hostname}:{lavaLinkSettings.Port}/v4/websocket");
                options.BaseAddress = baseAddress;
                options.WebSocketUri = webSocketUri;
                options.Passphrase = lavaLinkSettings.Password;
            })
            .AddLavalink()
            .AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); })
            .AddSingleton(botSettings)
            .AddSingleton<IFileSystem, PhysicalFileSystem>()
            .AddSingleton<DiscordClientEventHandler>()
            .AddSingleton(provider => DiscordClientFactory.Create(
                provider.GetRequiredService<BotSettings>(),
                provider.GetRequiredService<DiscordClientEventHandler>()))
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
            .AddSingleton<IPlaybackEventHandlerService, PlaybackEventHandlerService>()
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

        return services;
    }

    private static void RegisterSlashCommands(ServiceProvider services)
    {
        var discordClient = services.GetRequiredService<DiscordClient>();
        /* var slashCommandsConfig = discordClient.UseSlashCommands(new SlashCommandsConfiguration
         {
             Services = services
         });
         slashCommandsConfig.RefreshCommands();
         slashCommandsConfig.RegisterCommands<TagSlashCommand>();
         slashCommandsConfig.RegisterCommands<PingSlashCommand>();
         slashCommandsConfig.RegisterCommands<HelpSlashCommand>();
         slashCommandsConfig.RegisterCommands<PlaySlashCommand>();*/
    }

    private static void RegisterHandlers(ServiceProvider services)
    {
        var discordClient = services.GetRequiredService<DiscordClient>();
        var commandHandler = services.GetRequiredService<CommandHandlerService>();
        var reactionHandler = services.GetRequiredService<ReactionHandler>();

        commandHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);
    }
}