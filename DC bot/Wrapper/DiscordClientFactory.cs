using DC_bot.Configuration;
using DSharpPlus;

namespace DC_bot.Wrapper;

public static class DiscordClientFactory
{
    public static DiscordClient Create(BotSettings settings, DiscordClientEventHandler eventHandler)
    {
        var token = settings.Token ?? throw new Exception("DISCORD_TOKEN is not set.");

        var client = new DiscordClient(new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            AutoReconnect = true
        });

        client.Ready += eventHandler.OnClientReady;
        client.GuildAvailable += eventHandler.OnGuildAvailable;

        return client;
    }
}

