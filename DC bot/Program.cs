using DC_bot.Commands;
using DC_bot.Commands.SlashCommands;
using DC_bot.Interface;
using DC_bot.Service;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus.SlashCommands;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot;

internal class Program
{
    private static async Task Main()
    {
        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
        if (directoryInfo == null)
        {
            Console.WriteLine("Please specify a valid directory");
            return;
        }

        var envPath = Path.Combine(directoryInfo, ".env");
        Env.Load(envPath);
        await new Program().RunBotAsync();
    }

    private async Task RunBotAsync()
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("DISCORD_TOKEN is not set in the environment variables.");
            return;
        }

        var services = ConfigureServices();
        var botService = services.GetRequiredService<BotService>();

        RegisterSlashCommands();
        RegisterHandlers(services);

        await botService.StartAsync();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .ConfigureLavalink(options =>
            {
                var hostname = Environment.GetEnvironmentVariable("LAVALINK_HOSTNAME");
                var port = int.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT") ?? "2333");
                var baseAddress = new Uri($"https://{hostname}:{port}");
                var webSocketUri = new Uri($"wss://{hostname}:{port}/v4/websocket");
                options.BaseAddress = baseAddress;
                options.WebSocketUri = webSocketUri;
                options.Passphrase = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD");
            })   
            .AddSingleton(SingletonDiscordClient.Instance)      
            .AddLavalink()
            .AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); })
            .AddSingleton<BotService>()
            .AddSingleton<ReactionHandler>()
            .AddSingleton<CommandHandlerService>()
            .AddSingleton<ICommand, TagCommand>()
            .AddSingleton<ICommand, PingCommand>()
            .AddSingleton<ICommand, HelpCommand>()
            .AddSingleton<ICommand, PlayCommand>()
            .AddSingleton<ICommand, SkipCommand>()
            .AddSingleton<ICommand, PauseCommand>()
            .AddSingleton<ICommand, ResumeCommand>()
            .AddSingleton<ICommand, RepeatCommand>()
            .AddSingleton<ICommand, ShuffleCommand>()
            .AddSingleton<ICommand, LanguageCommand>()
            .AddSingleton<ICommand, ViewQueueCommand>()
            .AddSingleton<ICommand, RepeatListCommand>()
            .AddSingleton<ICommand, JoinCommand>()
            .AddSingleton<ILavaLinkService, LavaLinkService>()
            .AddSingleton<IMusicQueueService,MusicQueueService>()
            .AddSingleton<ILocalizationService, LocalizationService>()
            .AddSingleton<IUserValidationService, ValidationService>()
            .AddSingleton<IValidationService, ValidationService>()
            .BuildServiceProvider();

        var logger = services.GetRequiredService<ILogger<SingletonDiscordClient>>();
        SingletonDiscordClient.InitializeLogger(logger);
        ServiceLocator.SetServiceProvider(services);
        return services;
    }
   
    private static void RegisterSlashCommands()
    {
        var discordClient = SingletonDiscordClient.Instance;
        var slashCommandsConfig = discordClient.UseSlashCommands();
        slashCommandsConfig.RefreshCommands();
        slashCommandsConfig.RegisterCommands<TagSlashCommand>(1309813939563003966);
        slashCommandsConfig.RegisterCommands<PingSlashCommand>(1309813939563003966);
        slashCommandsConfig.RegisterCommands<HelpSlashCommand>(1309813939563003966);
        slashCommandsConfig.RegisterCommands<PlaySlashCommand>(1309813939563003966);
    }

    private static void RegisterHandlers(IServiceProvider services)
    {
        var discordClient = SingletonDiscordClient.Instance;
        var commandHandler = services.GetRequiredService<CommandHandlerService>();
        var reactionHandler = services.GetRequiredService<ReactionHandler>();

        commandHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);
    }
}