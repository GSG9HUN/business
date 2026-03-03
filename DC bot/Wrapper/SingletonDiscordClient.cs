using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Logging;
using DC_bot.Service;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class SingletonDiscordClient
{
    private static ILogger<SingletonDiscordClient> _logger = null!;
    private static string? _token;

    private static readonly Lazy<DiscordClient> _instance = new(() =>
    {
        var token = _token ?? throw new Exception("DISCORD_TOKEN is not set.");

        var client = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents | DiscordIntents.GuildMembers | DiscordIntents.Guilds,
            AutoReconnect = true
        });

        client.Ready += OnClientReady;
        client.GuildAvailable += OnGuildAvailable;

        return client;
    });

    public static DiscordClient Instance => _instance.Value;

    public static void InitializeSettings(BotSettings settings)
    {
        _token = settings.Token;
    }

    public static void InitializeLogger(ILogger<SingletonDiscordClient> logger)
    {
        _logger = logger;
        _logger.DiscordClientLoggerInitialized();
    }

    private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        try
        {
            _logger.DiscordClientReady();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private static async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        _logger.DiscordClientGuildAvailable(e.Guild.Name);

        var musicService = ServiceLocator.GetService<IMusicQueueService>();
        var lavaLinkService = ServiceLocator.GetService<ILavaLinkService>();
        var localizationService = ServiceLocator.GetService<ILocalizationService>();

        localizationService.LoadLanguage(e.Guild.Id);
        lavaLinkService.Init(e.Guild.Id);
        musicService.Init(e.Guild.Id);

        await lavaLinkService.ConnectAsync();

        await musicService.LoadQueue(e.Guild.Id);
    }
}