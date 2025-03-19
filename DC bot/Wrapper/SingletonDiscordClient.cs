using DC_bot.Interface;
using DC_bot.Service;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class SingletonDiscordClient
{
    private static ILogger<SingletonDiscordClient> _logger = null!;
    
    private static readonly Lazy<DiscordClient> _instance = new(() =>
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? throw new Exception("DISCORD_TOKEN is not set.");
        
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
    
    public static void InitializeLogger(ILogger<SingletonDiscordClient> logger)
    {
        _logger = logger;
        _logger.LogInformation("Logger initialized for SingletonDiscordClient.");
    }
    
    private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        _logger.LogInformation("Bot is ready!");
        return Task.CompletedTask;
    }

    private static async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        _logger.LogInformation($"Guild available: {e.Guild.Name}");

        var musicService = ServiceLocator.GetService<IMusicQueueService>();
        var lavaLinkService = ServiceLocator.GetService<ILavaLinkService>();
        var localizationService = ServiceLocator.GetService<ILocalizationService>();
        
        localizationService.LoadLanguage(e.Guild.Id);
        lavaLinkService.Init(e.Guild.Id);
        musicService.Init(e.Guild.Id);
        
        await lavaLinkService.ConnectAsync();
        var lavalink = Instance.GetLavalink();
        
        if (lavalink == null || !lavalink.ConnectedNodes.Any())
        {
            _logger.LogError("No connected Lavalink nodes available.");
            return;
        }
        
        var node = lavalink.ConnectedNodes.Values.FirstOrDefault();
        
        if (node == null) return;
        
        await musicService.LoadQueue(e.Guild.Id,node.Rest);
    }
}