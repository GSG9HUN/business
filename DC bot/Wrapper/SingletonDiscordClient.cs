using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class SingletonDiscordClient
{
    private static DiscordClient _instance;
    private static readonly object Lock = new();
    private static ILogger<SingletonDiscordClient> _logger;

    public static void InitializeLogger(ILogger<SingletonDiscordClient> logger)
    {
        _logger = logger;
        _logger.LogInformation("Logger initialized for SingletonDiscordClient.");
    }

    // Singleton instance létrehozása vagy elérése
    public static DiscordClient Instance
    {
        get
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
                    if (string.IsNullOrEmpty(token))
                    {
                        throw new Exception("DISCORD_TOKEN is not set in the environment variables.");
                    }

                    _instance = new DiscordClient(new DiscordConfiguration
                    {
                        Token = token,
                        TokenType = TokenType.Bot,
                        Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents |
                                  DiscordIntents.GuildMembers | DiscordIntents.Guilds,
                        AutoReconnect = true
                    });

                    _instance.Ready += OnClientReady;
                    _instance.GuildAvailable += OnGuildAvailable;

                    _logger.LogInformation("Singleton Discord Client is created.");
                }

                return _instance;
            }
        }
    }

    private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        _logger.LogInformation("Bot is ready!");
        return Task.CompletedTask;
    }

    private static Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        _logger.LogInformation($"Guild available: {e.Guild.Name}");
        return Task.CompletedTask;
    }
}