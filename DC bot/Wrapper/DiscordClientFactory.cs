using DC_bot.Configuration;
using DSharpPlus;

namespace DC_bot.Wrapper;

public static class DiscordClientFactory
{
    public static DiscordClient Create(BotSettings settings)
    {
        var token = settings.Token ?? throw new Exception("DISCORD_TOKEN is not set.");
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException("value");

        return DiscordClientBuilder
            .CreateDefault(token, DiscordIntents.All)
            .Build();
    }
}
